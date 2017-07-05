package main

import (
	"encoding/hex"
	"fmt"
	"strings"
	"sync"

	"github.com/pkg/term"
	"go.bug.st/serial.v1"
)

const NODE_ID = "3D"

var (
	port serial.Port

	remote   = false
	sendLock sync.Mutex // protects sendBuf between goroutine and main
	sendBuf  string
)

func main() {
	var e error
	device := "/dev/cu.usbmodem32212431"
	port, e = serial.Open(device, &serial.Mode{BaudRate: 115200})
	if e != nil {
		panic(e)
	}
	defer port.Close()
	fmt.Println("[Connected]")

	go handleSerialInput()

	handleConsoleInput()
}

// handleConsoleInput handles key input, either to send directly or over RF.
func handleConsoleInput() {
	t, e := term.Open("/dev/tty")
	if e != nil {
		panic(e)
	}
	defer t.Restore()
	term.RawMode(t)

	key := make([]byte, 1)
	for {
		n, e := t.Read(key)
		if e != nil || n == 0 {
			break
		}

		switch key[0] {
		case 0x04: // Ctrl-D, quit
			return
		case 0x12: // Ctrl-R, enter remote mode
			remote = true
			sendBuf = ""
			fmt.Print("\r\n[Remote]\r\n")
		case 0x0C: // Ctrl-L, exit remote mode, back to local
			remote = false
			fmt.Print("\r\n[Local]\r\n")
		default:
			if remote {
				sendLock.Lock()
				sendBuf += string(key)
				sendLock.Unlock()
			} else {
				port.Write(key)
			}
		}
	}
}

// handleSerialInput deals with all incoming serial data.
func handleSerialInput() {
	recvBuf := ""
	for {
		data := make([]byte, 250)
		n, e := port.Read(data)
		if e != nil {
			break
		}
		s := string(data[:n])
		if remote {
			recvBuf += s
			sendLock.Lock()
			if parsePacketMsg(recvBuf) {
				recvBuf = ""
			}
			sendLock.Unlock()
		} else {
			display(s)
		}
	}
}

// display shows received data while inserting a CR before each LF
func display(s string) {
	s = strings.Replace(s, "\n", "\r\n", -1)
	fmt.Print(s)
}

// parsePacketMsg recognises incoming RF69 data packets
func parsePacketMsg(s string) bool {
	if pos := strings.LastIndex(s, "RF69 "); pos >= 0 {
		s = s[pos:]
		if strings.Contains(s, "\n") {
			// RF69 21EE06AB01005EC0010A 8111F209D017994EB780\n
			f := strings.Fields(s)
			if len(f) == 3 && len(f[1]) == 20 && f[1][16:18] == NODE_ID {
				if b, e := hex.DecodeString(f[2]); e == nil {
					processPacket(b)
					return true
				}
			}
			fmt.Printf("%q ?\r\n", s)
		}
	}
	return false
}

// processPacket responds to each incoming packet by sending an ACK packet
func processPacket(p []byte) {
	display(string(p[1:]))
	msg := append([]byte{0}, []byte(sendBuf)...)
	if len(msg) > 66 {
		msg = msg[:66]
	}
	sendBuf = sendBuf[len(msg)-1:]
	fmt.Print(msg, "\r\n")
}
