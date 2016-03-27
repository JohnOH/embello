package main

import (
	"log"

	"github.com/tarm/serial"
	"gopkg.in/readline.v1"
)

var rlInstance *readline.Instance

func check(err error) {
	if err != nil {
		if rlInstance != nil {
			rlInstance.Close()
		}
		log.Fatal(err)
	}
}

func main() {
	rl, err := readline.NewEx(&readline.Config{
		UniqueEditLine: true,
	})
	check(err)
	rlInstance = rl
	defer rlInstance.Close()

	tty := "/dev/cu.SLAB_USBtoUART"
	serial, err := serial.OpenPort(&serial.Config{ Name: tty, Baud: 115200 })
	check(err);

	go func() {
		buf := make([]byte, 128)
		for {
			n, err := serial.Read(buf)
			check(err);
			print(string(buf[:n]))
			//rlInstance.Refresh()
		}
	}()

	serial.Write([]byte("\r"))
	for {
		line, err := rl.Readline()
		if err != nil { // io.EOF, readline.ErrInterrupt
			break
		}
		_, err = serial.Write([]byte(line + "\r"))
		check(err);
	}
}
