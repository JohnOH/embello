package main

import (
	"fmt"
	"os"
	"syscall"
	"time"
)

const (
	dev      = "/dev/i2c-1"
	addr     = 0x68
	I2CSLAVE = 0x0703
)

const (
	RF_CONFIG = iota
	RF_STATUS
	RF_PACKET
)

func main() {
	file, err := os.OpenFile(dev, os.O_RDWR, os.ModeExclusive)
	if err != nil {
		panic(err)
	}
	syscall.Syscall(syscall.SYS_IOCTL, file.Fd(), I2CSLAVE, addr)

	file.Write([]byte{RF_CONFIG, 8, 42, 1})
	file.Write([]byte{RF_STATUS})

	nbuf := make([]byte, 1)
	for {
		n, _ := file.Read(nbuf)
		if n > 0 && nbuf[0] > 0 {
			file.Write([]byte{RF_PACKET})
			buf := make([]byte, n)
			m, _ := file.Read(buf)
			file.Write([]byte{RF_STATUS})

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
