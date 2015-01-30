package main

import (
	"fmt"
	"os"
	"syscall"
	"time"
)

const (
	dev      = "/dev/i2c-1"
	addr     = 0x70
	I2CSLAVE = 0x0703
)

const (
	RF_STATUS = iota
	RF_PACKET
	RF_CONFIG
)

func main() {
	file, err := os.OpenFile(dev, os.O_RDWR, os.ModeExclusive)
	if err != nil {
		panic(err)
	}
	syscall.Syscall(syscall.SYS_IOCTL, file.Fd(), I2CSLAVE, addr)

	//file.Write([]byte{RF_CONFIG, 1, 42, 8})

	for {
		file.Write([]byte{RF_STATUS})
		nbuf := make([]byte, 1)
		n, _ := file.Read(nbuf)
		if n > 0 && nbuf[0] > 0 {
			file.Write([]byte{RF_PACKET})
			buf := make([]byte, n)
			m, _ := file.Read(buf)

			if m == n {
				fmt.Printf("%02X\n", buf)
			} else {
				fmt.Println("n?", n, m)
			}
		} else {
			time.Sleep(100 * time.Millisecond)
		}
	}
}
