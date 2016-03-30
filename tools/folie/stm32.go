package main

import (
	"fmt"
	"os"
	"time"
)

const (
	ACK        = 0x79
	NAK        = 0x1F
	GET_CMD    = 0x00
	GETID_CMD  = 0x02
	WRITE_CMD  = 0x31
	ERASE_CMD  = 0x43
	EXTERA_CMD = 0x44
	WRUNP_CMD  = 0x73
	RDUNP_CMD  = 0x92
)

var deviceMap = map[uint16]string{
	0x410: "STM32F1, performance, medium-density",
	0x411: "STM32F2",
	0x412: "STM32F1, performance, low-density",
	0x413: "STM32F4",
	0x414: "STM32F1, performance, high-density",
	0x416: "STM32L1, performance, medium-density",
	0x418: "STM32F1, connectivity",
	0x420: "STM32F1, value, medium-density",
	0x428: "STM32F1, value, high-density",
	0x430: "STM32F1, performance, XL-density",
}

var (
	checkSum uint8
	pending  []byte
	extended bool
)

func uploadSTM32(data []byte) {
	fmt.Print(" Synchronise: ")
	connectToTarget()
	fmt.Println(" OK")

	fmt.Printf(" Boot loader: %02x hex\n", getBootVersion())

	chip := getChipType()
	desc := ""
	if s, ok := deviceMap[chip]; ok {
		desc = " - " + s
	}
	fmt.Printf("   Chip type: %04x hex%s\n", chip, desc)

	fmt.Print("   Unprotect: ")
	sendCmd(RDUNP_CMD)
	wantAck()
	fmt.Println("OK")

	fmt.Print("      Resume: ")
	connectToTarget()
	fmt.Println(" OK")

	//fmt.Print(" WR-unprotect: ")
	//sendCmd(WRUNP_CMD)
	//wantAck()
	//fmt.Println("OK")

	//fmt.Print("      Resume: ")
	//connectToTarget()
	//fmt.Println(" OK")

	fmt.Print("  Mass erase: ")
	massErase()
	fmt.Println("OK")

	fmt.Print("   Uploading: ")
	writeFlash(data)
	fmt.Println(" OK")
}

func getReply() uint8 {
	if len(pending) == 0 {
		pending = readWithTimeout()
	}
	if len(pending) == 0 {
		return 0
	}
	b := pending[0]
	if *verbose {
		fmt.Printf("<%02x", b)
	}
	pending = pending[1:]
	return b
}

func connectToTarget() {
	for {
		conn.Flush()
		fmt.Print(".") // auto-baud greeting
		sendByte(0x7F)
		r := getReply()
		if r == ACK || r == NAK {
			if r == ACK {
				fmt.Print("+")
			}
			break
		}
		time.Sleep(time.Second)
	}
	// got a valid reply
}

func wantAck() {
	r := getReply()
	if r != ACK {
		fmt.Printf("\nFailed: %02x\n", r)
		os.Exit(1)
	}
	checkSum = 0
}

func sendByte(b uint8) {
	if *verbose {
		fmt.Printf(">%02x", b)
	}
	conn.Write([]byte{b})
	checkSum ^= b
}

func sendCmd(cmd uint8) {
	//getReply()  // get rid of pending data
	conn.Flush()
	pending = nil
	sendByte(cmd)
	sendByte(^cmd)
	wantAck()
}

func getBootVersion() uint8 {
	sendCmd(GET_CMD)
	n := getReply()
	rev := getReply()
	for i := 0; i < int(n); i++ {
		if getReply() == EXTERA_CMD {
			extended = true
		}
	}
	wantAck()
	return rev
}

func getChipType() uint16 {
	sendCmd(GETID_CMD)
	getReply() // should be 1
	chipType := uint16(getReply()) << 8
	chipType |= uint16(getReply())
	wantAck()
	return chipType
}

func massErase() {
	if extended {
		sendCmd(EXTERA_CMD)
		sendByte(0xFF)
		sendByte(0xFF)
		sendByte(0xFF)
	} else {
		sendCmd(ERASE_CMD)
		sendByte(0xFF)
		sendByte(0x00)
	}
	wantAck()
}

func writeFlash(data []byte) {
	for len(data)%256 != 0 {
		data = append(data, 0xFF)
	}
	for offset := 0; offset < len(data); offset += 256 {
		fmt.Print("+")
		sendCmd(WRITE_CMD)
		addr := 0x08000000 + offset
		sendByte(uint8(addr >> 24))
		sendByte(uint8(addr >> 16))
		sendByte(uint8(addr >> 8))
		sendByte(uint8(addr))
		sendByte(checkSum)
		wantAck()
		sendByte(256 - 1)
		for i := 0; i < 256; i++ {
			sendByte(data[offset+i])
		}
		sendByte(checkSum)
		wantAck()
		*verbose = false // verbose mode off after one write, to reduce output
	}
}
