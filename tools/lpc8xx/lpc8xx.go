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
	termFlag   = flag.Bool("t", false, "launch as serial terminal after upload")
	waitFlag   = flag.Bool("w", false, "wait for connection to the boot loader")
	offsetFlag = flag.Int("o", 0, "upload offset (must be a multiple of 1024)")
	idleFlag   = flag.Int("i", 0, "exit terminal after N idle seconds (0: off)")
	netFlag    = flag.Bool("n", false, "use telnet protocol for RTS & DTR")
)

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
	fmt.Printf("found: %X\n", id)

	conn.SendAndWait("N", "0")
	buf := bytes.NewBuffer([]byte{})

	for i := 0; i < 4; i++ {
		b, err := strconv.ParseUint(<-conn.lines, 10, 32)
		Check(err)
		binary.Write(buf, binary.LittleEndian, uint32(b))
	}
	fmt.Printf("hwuid: %X\n", buf.Bytes())

	if binFile != "" {
		data, err := ioutil.ReadFile(binFile)
		Check(err)

		fmt.Print("flash: 00000 ")
		for n := range conn.Program(*offsetFlag, data) {
			fmt.Printf("\b\b\b\b\b\b%05d ", n*64)
		}
		fmt.Println("done")
	}

	conn.serial.SetDTR(true) // pulse DTR to reset
	conn.serial.SetDTR(false)

	if *termFlag {
		fmt.Println("entering terminal mode, press <ESC> to quit:\n")
		conn.Terminal()
		fmt.Println()
	}
}

func connect(port string) *connection {
	var dev serialLink

	if _, err := os.Stat(port); os.IsNotExist(err) {
		sock, err := net.Dial("tcp", port)
		Check(err)
		dev = &telnet{sock}
	} else {
		opt := rs232.Options{BitRate: 115200, DataBits: 8, StopBits: 1}
		dev, err = rs232.Open(port, opt)
		Check(err)
	}

	conn := &connection{dev, make(chan string)}
	go func() {
		scanner := bufio.NewScanner(dev)
		for scanner.Scan() {
			conn.lines <- scanner.Text()
		}
		close(conn.lines)
	}()
	return conn
}

type serialLink interface {
	io.ReadWriter
	SetDTR(level bool) error
	SetRTS(level bool) error
}

type telnet struct {
	net.Conn
}

func (c *telnet) SetDTR(level bool) error {
	if *netFlag {
		// TODO proper byte sequence
		_, err := net.Conn(c).Write([]byte("<DTR>"))
		return err
	}
	return nil
}

func (c *telnet) SetRTS(level bool) error {
	if *netFlag {
		// TODO proper byte sequence
		_, err := net.Conn(c).Write([]byte("<RTS>"))
		return err
	}
	return nil
}

func (c *telnet) Read(p []byte) (n int, err error) {
	// TODO read, unescape, and ignore in-band data
	return 0, io.EOF
}

func (c *telnet) Write(b []byte) (int, error) {
	escaped := bytes.Replace(b, []byte{0xFF}, []byte{0xFF, 0xFF}, -1)
	_, err := net.Conn(c).Write(escaped)
	return len(b), err
}

type connection struct {
	serial serialLink
	lines  chan string
}

func (c *connection) ReadReply() string {
	select {
	case reply := <-c.lines:
		return reply
	case <-time.After(250 * time.Millisecond):
		return ""
	}
}

func (c *connection) SendAndWait(cmd string, expect string) {
	c.serial.Write([]byte(cmd + "\r\n"))
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

func (c *connection) Identify() int {
	c.serial.SetRTS(true) // keep RTS on for ISP mode

	for {
		c.serial.SetDTR(true) // pulse DTR to reset
		for c.ReadReply() != "" {
			// flush
		}
		c.serial.SetDTR(false)

		c.serial.Write([]byte("?\r\n"))
		if c.ReadReply() == "Synchronized" || !*waitFlag {
			break
		}
	}

	c.serial.SetRTS(false)

	c.SendAndWait("Synchronized", "OK")
	c.SendAndWait("12000", "OK")
	c.SendAndWait("A 0", "0")

	c.SendAndWait("J", "0")
	id, err := strconv.Atoi(c.ReadReply())
	Check(err)

	return id
}

func (c *connection) Program(startAddress int, data []byte) chan int {
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
			c.serial.Write(data[offset : offset+pageSize])
			// prepare and copy the data to flash memory
			sector := (page * pageSize) / sectorSize
			c.SendAndWait(fmt.Sprint("P ", sector, sector), "0")
			destAddr := page * pageSize
			c.SendAndWait(fmt.Sprint("C ", destAddr, RAM_ADDR, pageSize), "0")
			r <- (page - firstPage) + 1
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

func (c *connection) Terminal() {
	timeout := time.Duration(*idleFlag) * time.Second
	idleTimer := time.NewTimer(timeout)

	// FIXME still in line mode, so only complete lines will be shown
	//  look into bufio.Reader for a possible solution
	go func() {
		for s := range c.lines {
			idleTimer.Reset(timeout)
			fmt.Println(s)
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
		c.serial.Write(b[:n])
	}

}

func Check(e error) {
	if e != nil {
		log.Fatal(e)
	}
}
