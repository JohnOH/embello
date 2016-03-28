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
	RDUNP_CMD  = 0x92
)

var (
	checkSum uint8
	pending  []byte
)

func uploadSTM32(data []byte) {
	connectToTarget()
	fmt.Println("OK")

	fmt.Printf("Boot version: %02x\n", getBootVersion())
	fmt.Printf("Chip type: %04x\n", getChipType())

	fmt.Print("Unprotect: ")
	sendCmd(RDUNP_CMD)
	wantAck()
	fmt.Println("OK")

	fmt.Print("Resuming: ")
	connectToTarget()
	fmt.Println("OK")

	fmt.Print("Mass erase: ")
	massErase()
	fmt.Println("OK")

    fmt.Print("Writing: ");
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
	pending = pending[1:]
	return b
}

func connectToTarget() {
	for {
		conn.Flush()
		fmt.Print("?") // auto-baud greeting
		conn.Write([]byte{0x7F})
		r := getReply()
		if r == ACK || r == NAK {
			break
		}
		time.Sleep(time.Second)
	}
	fmt.Print("! ") // got a valid reply
}

func wantAck() {
	r := getReply()
	if r != ACK {
		fmt.Println()
		fmt.Println("failed:", r)
		os.Exit(1)
	}
	checkSum = 0
}

func sendByte(b uint8) {
	conn.Write([]byte{b})
	checkSum ^= b
}

func sendCmd(cmd uint8) {
	sendByte(cmd)
	sendByte(^cmd)
	wantAck()
}

func getBootVersion() uint8 {
	sendCmd(GET_CMD)
	n := getReply()
	rev := getReply()
	for i := 0; i < int(n); i++ {
		getReply()
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
	if true {
		sendCmd(ERASE_CMD)
		sendByte(0xFF)
		sendByte(0x00)
		wantAck()
	} else {
		sendCmd(EXTERA_CMD)
		sendByte(0xFF)
		sendByte(0xFF)
		sendByte(0xFF)
		wantAck()
	}
}

func writeFlash(data []byte) {
	for len(data) % 256 != 0 {
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
        sendByte(256-1)
        for i := 0; i < 256; i++ {
            sendByte(data[offset+i])
		}
        sendByte(checkSum)
        wantAck()
    }
}
