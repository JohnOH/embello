package upload

import (
	"bufio"
	"bytes"
	"encoding/binary"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"strconv"
	"time"

	"github.com/chimera/rs232"
	"github.com/codegangsta/cli"
	"github.com/jeelabs/embello/tools/jetx/cmd"
)

var (
	target   *connection
	quitChan = make(chan bool)
)

func init() {
	c := cmd.Define("upload", "upload firmware to target system", upload)
	c.Flags = []cli.Flag{
		cli.StringFlag{
			Name:   "port, p",
			Value:  "/dev/ttyUSB0",
			Usage:  "usb port to connect to",
			EnvVar: "JET_USBPORT",
		},
	}
}

func upload(c *cli.Context) {
	if len(c.Args()) != 1 {
		cmd.Fatalf("upload: which firmware file?")
	}
	fwData, err := ioutil.ReadFile(c.Args()[0])
	check(err)

	serialPort := c.String("port")
	fmt.Println("Connecting to", serialPort)
	target = connect(serialPort)

	id := target.Identify()
	fmt.Printf("Found LPC %X.%X\n", id>>4, id&0xF)

	fmt.Print("Uploading ", len(fwData), " bytes ")
	for _ = range target.Program(0x0000, fwData) {
		fmt.Print("+")
	}
	fmt.Println(" done")

	target.SetDTR(true) // pulse DTR to reset
	target.SetDTR(false)
}

func connect(port string) *connection {
	opt := rs232.Options{BitRate: 115200, DataBits: 8, StopBits: 1}
	target, err := rs232.Open(port, opt)
	check(err)

	ch := make(chan []byte)

	go func() {
		var buf [100]byte
		for {
			n, err := target.Read(buf[:])
			if err != nil {
				break
			}
			ch <- buf[:n]
		}
	}()

	pr, pw := io.Pipe()

	go func() {
		for {
			select {
			case data := <-ch:
				pw.Write(data)
			case <-quitChan:
				pw.Close() // this will stop the scanner, yeay!
				return
			}
		}
	}()

	ln := make(chan string)

	go func() {
		scanner := bufio.NewScanner(pr)
		for scanner.Scan() {
			ln <- scanner.Text()
		}
	}()

	return &connection{target, ln, ch}
}

type connection struct {
	*rs232.Port
	lines chan string
	bytes chan []byte
}

func (c *connection) SendAndWait(cmd string, expectList ...string) {
	c.Write([]byte(cmd + "\r\n"))
	for _, expect := range expectList {
		select {
		case reply := <-c.lines:
			if reply != expect {
				log.Panicln(reply, expect, cmd)
			}
		case <-time.After(2 * time.Second):
			panic("timeout!")
		}
	}
}

func (c *connection) Identify() int {
	// TODO: this is the wrong place, needs to move higher up
	// defer func() {
	//     if e, ok := recover().(error); ok {
	//         err = e
	//     }
	// }()

	c.SetRTS(true) // keep RTS on for ISP mode
	c.SetDTR(true) // pulse DTR to reset
	c.SetDTR(false)
	time.Sleep(10 * time.Millisecond)
	c.SetRTS(false)

	c.SendAndWait("?", "Synchronized")
	c.SendAndWait("Synchronized", "Synchronized", "OK")
	c.SendAndWait("12000", "12000", "OK")
	c.SendAndWait("A 0", "A 0", "0")

	c.SendAndWait("J", "0")
	id, err := strconv.Atoi(<-c.lines)
	check(err)

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
			c.Write(data[offset : offset+pageSize])
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
	check(err)
	var sum uint32
	for _, v := range values {
		sum -= v
		// fmt.Println(v, sum)
	}
	data[28] = byte(sum)
	data[29] = byte(sum >> 8)
	data[30] = byte(sum >> 16)
	data[31] = byte(sum >> 24)
}

func check(err error) {
	if err != nil {
		cmd.Fatalf(err.Error())
	}
}
