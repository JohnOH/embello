// Quick and dirty uploader and serial terminal for LPC8xx chips.
// -jcw, 2015-02-02

package main

import (
	"flag"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"net"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/chimera/rs232"
	"github.com/jeelabs/embello/tools/uploader/serflash"
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

func main() {
	log.SetFlags(0) // no timestamps

	flag.Usage = func() {
		fmt.Fprintln(os.Stderr, "Usage: uploader ?options? tty ?binfile?")
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

	id, info, hwuid := conn.Identify()
	fmt.Printf("found: %X - %s\n", id, info)
	fmt.Printf("hwuid: %X\n", hwuid)

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

func connect(port string) *serflash.Conn {
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
		dev = serflash.UseTelnet(dev)
	}

	return serflash.New(dev, *debugFlag, *waitFlag)
}

func terminalMode(c *serflash.Conn) {
	timeout := time.Duration(*idleFlag) * time.Second
	idleTimer := time.NewTimer(timeout)

	// FIXME still in line mode, so only complete lines will be shown
	go func() {
		for line := range c.Lines {
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
