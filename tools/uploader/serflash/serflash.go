package serflash

import (
	"bufio"
	"bytes"
	"encoding/binary"
	"fmt"
	"io"
	"log"
	"strconv"
	"strings"
	"time"

	"github.com/jeelabs/embello/tools/uploader/lpc8xx"
)

// New creates a connection for uploading and terminal session use.
func New(rw io.ReadWriter, debug, wait bool) *Conn {
	brd := bufio.NewReader(rw)
	ctl, ok := rw.(controllable)
	if !ok {
		// if the reader does not support DTR/RTS, use a dummy one
		ctl = new(dummyControllable)
	}
	conn := &Conn{make(chan string), brd, rw, ctl, debug, wait}

	go func() {
		for {
			line, err := brd.ReadString('\n')
			Check(err)
			if debug {
				fmt.Printf("R: %q\n", line)
			}
			n := len(line)
			if n >= 2 && line[n-2] == '\r' {
				line = line[:n-2]
			}
			conn.Lines <- line
		}
	}()

	return conn
}

// controllable can set the DTR and RTS levels
type controllable interface {
	SetDTR(level bool) error
	SetRTS(level bool) error
}

// dummyControllable silently ignores all DTR/RTS request
type dummyControllable struct{}

func (c *dummyControllable) SetDTR(level bool) error {
	return nil
}

func (c *dummyControllable) SetRTS(level bool) error {
	return nil
}

// Conn is a buffered reader, unbuffered writer, and DTR/RTS controllable.
type Conn struct {
	Lines chan string

	*bufio.Reader
	io.Writer
	controllable

	debug, wait bool
}

func (c *Conn) readReply() (string, bool) {
	select {
	case line := <-c.Lines:
		return line, true
	case <-time.After(2 * time.Second):
		return "", false
	}
}

func (c *Conn) sendAndWait(cmd string, expect string) {
	if c.debug {
		fmt.Printf("W: %q\n", cmd+"\r\n")
	}
	c.Write([]byte(cmd + "\r\n"))
	var reply string
	for i := 0; i < 4; i++ {
		reply, ok := c.readReply()
		if !ok {
			log.Fatal("no response, timeout")
		}
		if reply == expect {
			return
		}
	}
	log.Fatal(reply)
}

func (c *Conn) Identify() (int, string, []byte) {
	c.SetRTS(true) // keep RTS on for ISP mode

	for {
		c.SetDTR(true)                     // pulse DTR to reset
		c.Read(make([]byte, c.Buffered())) // flush
		c.SetDTR(false)

		c.Write([]byte("?\r\n"))
		reply, ok := c.readReply()
		if strings.HasSuffix(reply, "Synchronized") {
			break
		}

		if !ok && !c.wait {
			break // no sync, will fail later - after RTS has been restored
		}
	}

	c.SetRTS(false)

	c.sendAndWait("Synchronized", "OK")
	c.sendAndWait("12000", "OK")
	c.sendAndWait("A 0", "0")

	c.sendAndWait("J", "0")
	reply, _ := c.readReply()
	id, err := strconv.Atoi(reply)
	Check(err)

	c.sendAndWait("N", "0")

	buf := bytes.NewBuffer([]byte{})
	for i := 0; i < 4; i++ {
		reply, _ = c.readReply()
		b, err := strconv.ParseUint(reply, 10, 32)
		Check(err)
		binary.Write(buf, binary.LittleEndian, uint32(b))
	}

	info, _ := lpc8xx.ChipInfo[id]

	return id, info, buf.Bytes()
}

func (c *Conn) Program(startAddress int, data []byte) chan int {
	const sectorSize = 1024
	const pageSize = 64

	for len(data)%pageSize != 0 {
		data = append(data, 0xFF)
	}
	firstPage := startAddress / pageSize
	lastPage := firstPage + len(data)/pageSize - 1

	for len(data)%sectorSize != 0 {
		data = append(data, 0xFF)
	}
	firstSector := startAddress / sectorSize
	lastSector := firstSector + len(data)/sectorSize - 1

	if startAddress == 0 {
		fixChecksum(data)
	}

	c.sendAndWait("U 23130", "0") // unlock for programming

	// erase entire range, as 1024-byte sectors
	c.sendAndWait(fmt.Sprint("P ", firstSector, lastSector), "0") // prepare
	c.sendAndWait(fmt.Sprint("E ", firstSector, lastSector), "0") // erase

	r := make(chan int)
	go func() {
		defer close(r)
		// program in 64-byte pages (sectors won't fit in the LPC810's RAM)
		for page := firstPage; page <= lastPage; page++ {
			const RAM_ADDR = 0x10000300
			// write one page of data to RAM
			offset := (page - firstPage) * pageSize
			c.sendAndWait(fmt.Sprint("W ", RAM_ADDR, pageSize), "0")
			c.Write(data[offset : offset+pageSize])
			// prepare and copy the data to flash memory
			sector := (page * pageSize) / sectorSize
			c.sendAndWait(fmt.Sprint("P ", sector, sector), "0")
			destAddr := page * pageSize
			c.sendAndWait(fmt.Sprint("C ", destAddr, RAM_ADDR, pageSize), "0")
			r <- pageSize * (page - firstPage + 1)
		}
	}()
	return r
}

// fix the checksum to mark code as valid (see UM10398, p.416)
func fixChecksum(data []byte) {
	buf := bytes.NewReader(data)
	values := [7]uint32{}
	err := binary.Read(buf, binary.LittleEndian, values[:])
	Check(err)
	var sum uint32
	for _, v := range values {
		sum -= v
	}
	data[28] = byte(sum)
	data[29] = byte(sum >> 8)
	data[30] = byte(sum >> 16)
	data[31] = byte(sum >> 24)
}

func UseTelnet(s io.ReadWriter) io.ReadWriter {
	// doesn't seem to be needed:
	s.Write([]byte{Iac, Will, ComPortOpt})
	return &telnetWrapper{upLink: s, inState: 0}
}

// telnetWrapper turns RTS/DTR signals into in-band telnet requests
type telnetWrapper struct {
	upLink  io.ReadWriter
	inState int
}

const (
	Iac  = 255
	Will = 251
	Sb   = 250
	Se   = 240

	ComPortOpt = 44
	SetControl = 5
)

func (c *telnetWrapper) SetDTR(level bool) error {
	return c.sendEscape(level, 8, 9)
}

func (c *telnetWrapper) SetRTS(level bool) error {
	return c.sendEscape(level, 11, 12)
}

func (c *telnetWrapper) sendEscape(flag bool, yes, no uint8) error {
	b := no
	if flag {
		b = yes
	}

	_, err := c.upLink.Write([]byte{Iac, Sb, ComPortOpt, SetControl, b, Iac, Se})
	return err
}

func (c *telnetWrapper) Read(buf []byte) (n int, err error) {
	j := 0
	for {
		n, err := c.upLink.Read(buf)
		if err != nil {
			return n, err
		}
		for i := 0; i < n; i++ {
			b := buf[i]
			buf[j] = b
			switch c.inState {
			case 0: // normal, copying
				if b == Iac {
					c.inState = 1
				} else {
					j++
				}
			case 1: // seen Iac
				if b == Sb {
					c.inState = 2
				} else {
					j++
					c.inState = 0
				}
			case 2: // inside command
				if b == Iac {
					c.inState = 3
				}
			case 3: // inside command, see Iac
				if b == Se {
					c.inState = 0
				} else {
					c.inState = 2
				}
			}
		}
		if j > 0 {
			break
		}
	}
	return j, nil
}

func (c *telnetWrapper) Write(buf []byte) (int, error) {
	wrapped := bytes.Replace(buf, []byte{0xFF}, []byte{0xFF, 0xFF}, -1)
	// FIXME returned count is wrong
	n, err := c.upLink.Write(wrapped)
	if n > len(buf) {
		n = len(buf)
	}
	return n, err
}

func Check(e error) {
	if e != nil {
		log.Fatal(e)
	}
}
