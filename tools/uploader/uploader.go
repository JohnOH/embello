// Quick and dirty uploader and serial terminal for LPC8xx chips.
// -jcw, 2015-02-02

package main

import (
	"bufio"
	"bytes"
	"encoding/binary"
	"flag"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"net"
	"os"
	"os/signal"
	"strconv"
	"syscall"
	"time"

	"github.com/chimera/rs232"
	"golang.org/x/crypto/ssh/terminal"
)

var (
	offsetFlag = flag.Int("o", 0, "upload offset (must be multiple of 1024)")
	serialFlag = flag.Bool("s", false, "launch as serial terminal after upload")
	waitFlag   = flag.Bool("w", false, "wait for connection to the boot loader")
	idleFlag   = flag.Int("i", 0, "exit terminal after N idle seconds (0: off)")
	telnetFlag = flag.Bool("t", false, "use telnet protocol for RTS & DTR")
	debugFlag  = flag.Bool("d", false, "verbose debugging output")
)

var chipInfo = map[int]string{
	0x8100: "- LPC810: 4 KB flash, 1 KB RAM, DIP8",
	0x8110: "- LPC811: 8 KB flash, 2 KB RAM, TSSOP16",
	0x8120: "- LPC812: 16 KB flash, 4 KB RAM, TSSOP16",
	0x8121: "- LPC812: 16 KB flash, 4 KB RAM, SO20",
	0x8122: "- LPC812: 16 KB flash, 4 KB RAM, TSSOP20",
	0x8221: "- LPC822: 16 KB flash, 4 KB RAM, HVQFN33",
	0x8222: "- LPC822: 16 KB flash, 4 KB RAM, TSSOP20",
	0x8241: "- LPC824: 32 KB flash, 8 KB RAM, HVQFN33",
	0x8242: "- LPC824: 32 KB flash, 8 KB RAM, TSSOP20",
}

func main() {
	log.SetFlags(0) // no timestamps

	flag.Usage = func() {
		fmt.Fprintln(os.Stderr, "Usage: lpc8xx ?options? tty ?binfile?")
		flag.PrintDefaults()
	}

	flag.Parse()

	if flag.NArg() < 1 {
		flag.Usage()
		os.Exit(2)
	}

	ttyName := flag.Arg(0)
	binFile := ""
	if flag.NArg() > 1 {
		binFile = flag.Arg(1)
	}

	conn := connect(ttyName)

	id := conn.Identify()
	info, _ := chipInfo[id]
	fmt.Printf("found: %X %s\n", id, info)

	conn.SendAndWait("N", "0")

	buf := bytes.NewBuffer([]byte{})
	for i := 0; i < 4; i++ {
		b, err := strconv.ParseUint(conn.ReadReply(), 10, 32)
		Check(err)
		binary.Write(buf, binary.LittleEndian, uint32(b))
	}
	fmt.Printf("hwuid: %X\n", buf.Bytes())

	if binFile != "" {
		data, err := ioutil.ReadFile(binFile)
		Check(err)

		fmt.Print("flash: 0000 ")
		for n := range conn.Program(*offsetFlag, data) {
			fmt.Printf("\b\b\b\b\b%04X ", n)
			if *debugFlag {
				fmt.Println()
			}
		}
		fmt.Println("done,", len(data), "bytes")
	}

	conn.SetDTR(true) // pulse DTR to reset
	conn.SetDTR(false)

	if *serialFlag {
		*debugFlag = false
		fmt.Println("entering terminal mode, press <ESC> to quit:\n")
		terminalMode(conn)
		fmt.Println()
	}
}

// controllable can set the DTR and RTS levels
type controllable interface {
	SetDTR(level bool) error
	SetRTS(level bool) error
}

func connect(port string) *SerConn {
	var dev io.ReadWriter

	if _, err := os.Stat(port); os.IsNotExist(err) {
		// if nonexistent, it's an ip address + port and open as network port
		dev, err = net.Dial("tcp", port)
		Check(err)
		// RTS and DTR will be ignored unless telnet is specified
	} else {
		// else assume the tty is an existing device, open as rs232 port
		opt := rs232.Options{BitRate: 115200, DataBits: 8, StopBits: 1}
		dev, err = rs232.Open(port, opt)
		Check(err)
	}

	if *telnetFlag {
		dev = wrapAsTelnet(dev)
	}

	return NewSerConn(dev)
}

func wrapAsTelnet(s io.ReadWriter) io.ReadWriter {
	// doesn't seem to be needed:
	//s.Write([]byte{Iac, Will, ComPortOpt})
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

// NewConnection creates a connection for uploading and terminal session use.
func NewSerConn(rw io.ReadWriter) *SerConn {
	brd := bufio.NewReader(rw)
	ctl, ok := rw.(controllable)
	if !ok {
		// if the reader does not support DTR/RTS, use a dummy one
		ctl = new(dummyControllable)
	}
	conn := &SerConn{brd, rw, ctl, make(chan string)}

	go func() {
		for {
			line, err := brd.ReadString('\n')
			Check(err)
			if *debugFlag {
				fmt.Printf("R: %q\n", line)
			}
			n := len(line)
			if n >= 2 && line[n-2] == '\r' {
				line = line[:n-2]
			}
			conn.lines <- line
		}
	}()

	return conn
}

// dummyControllable silently ignores all DTR/RTS request
type dummyControllable struct{}

func (c *dummyControllable) SetDTR(level bool) error {
	return nil
}

func (c *dummyControllable) SetRTS(level bool) error {
	return nil
}

// SerConn is a buffered reader, unbuffered writer, and DTR/RTS controllable.
type SerConn struct {
	*bufio.Reader
	io.Writer
	controllable
	lines chan string
}

func (c *SerConn) ReadReply() string {
	select {
	case line := <-c.lines:
		return line
	case <-time.After(2000 * time.Millisecond):
		return ""
	}
}

func (c *SerConn) SendAndWait(cmd string, expect string) {
	if *debugFlag {
		fmt.Printf("W: %q\n", cmd+"\r\n")
	}
	c.Write([]byte(cmd + "\r\n"))
	var reply string
	for i := 0; i < 4; i++ {
		reply = c.ReadReply()
		if reply == "" {
			log.Fatal("no response, timeout")
		}
		if reply == expect {
			return
		}
	}
	log.Fatal(reply)
}

func (c *SerConn) Identify() int {
	c.SetRTS(true) // keep RTS on for ISP mode

	for {
		c.SetDTR(true)                     // pulse DTR to reset
		c.Read(make([]byte, c.Buffered())) // flush
		c.SetDTR(false)

		c.Write([]byte("?\r\n"))
		if c.ReadReply() == "Synchronized" || !*waitFlag {
			break
		}
	}

	c.SetRTS(false)

	c.SendAndWait("Synchronized", "OK")
	c.SendAndWait("12000", "OK")
	c.SendAndWait("A 0", "0")

	c.SendAndWait("J", "0")
	id, err := strconv.Atoi(c.ReadReply())
	Check(err)

	return id
}

func (c *SerConn) Program(startAddress int, data []byte) chan int {
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

	c.SendAndWait("U 23130", "0") // unlock for programming

	// erase entire range, as 1024-byte sectors
	c.SendAndWait(fmt.Sprint("P ", firstSector, lastSector), "0") // prepare
	c.SendAndWait(fmt.Sprint("E ", firstSector, lastSector), "0") // erase

	r := make(chan int)
	go func() {
		defer close(r)
		// program in 64-byte pages (sectors won't fit in the LPC810's RAM)
		for page := firstPage; page <= lastPage; page++ {
			const RAM_ADDR = 0x10000300
			// write one page of data to RAM
			offset := (page - firstPage) * pageSize
			c.SendAndWait(fmt.Sprint("W ", RAM_ADDR, pageSize), "0")
			c.Write(data[offset : offset+pageSize])
			// prepare and copy the data to flash memory
			sector := (page * pageSize) / sectorSize
			c.SendAndWait(fmt.Sprint("P ", sector, sector), "0")
			destAddr := page * pageSize
			c.SendAndWait(fmt.Sprint("C ", destAddr, RAM_ADDR, pageSize), "0")
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

func terminalMode(c *SerConn) {
	timeout := time.Duration(*idleFlag) * time.Second
	idleTimer := time.NewTimer(timeout)

	// FIXME still in line mode, so only complete lines will be shown
	go func() {
		for line := range c.lines {
			idleTimer.Reset(timeout)
			fmt.Println(line)
		}
	}()

	// put stdin in raw mode
	oldState, err := terminal.MakeRaw(0)
	Check(err)
	defer terminal.Restore(0, oldState)

	// cleanup when program is terminated via a signal
	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, os.Interrupt, syscall.SIGHUP, syscall.SIGTERM)
	go func() {
		sigMsg := <-sigChan
		terminal.Restore(0, oldState)
		log.Fatal(sigMsg)
	}()

	// cleanup when idle timer fires, and exit cleanly
	if *idleFlag > 0 {
		go func() {
			<-idleTimer.C
			terminal.Restore(0, oldState)
			fmt.Println("\nidle timeout")
			os.Exit(0)
		}()
	}

	// copy key presses to the serial port
	for {
		var b [1]byte
		n, _ := os.Stdin.Read(b[:])
		idleTimer.Reset(timeout)
		if n < 1 || b[0] == 0x1B { // ESC
			break
		}
		c.Write(b[:n])
	}

}

func Check(e error) {
	if e != nil {
		log.Fatal(e)
	}
}
