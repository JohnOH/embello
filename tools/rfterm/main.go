package main

import (
	"fmt"
	"strings"

	"github.com/pkg/term"
	"go.bug.st/serial.v1"
)

func main() {
	t, e1 := term.Open("/dev/tty")
	if e1 != nil {
		panic(e1)
	}
	defer t.Restore()
	term.RawMode(t)

	port := "/dev/cu.usbmodem32212431"
	tty, e2 := serial.Open(port, &serial.Mode{BaudRate: 115200})
	if e2 != nil {
		panic(e2)
	}
	defer tty.Close()

	go func() {
		for {
			data := make([]byte, 250)
			n, err := tty.Read(data)
			if err != nil {
				break
			}
			s := string(data[:n])
			s = strings.Replace(s, "\n", "\r\n", -1)
			fmt.Print(s)
		}
	}()

	key := make([]byte, 1)
	for {
		n, e := t.Read(key)
		if e != nil || n == 0 || key[0] == 0x04 {
			break
		}
		tty.Write(key)
	}
}
