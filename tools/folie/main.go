package main

import (
	"fmt"
	"log"
	"strings"

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
	var err error
	rlInstance, err = readline.NewEx(&readline.Config{
		UniqueEditLine: true,
	})
	check(err)
	defer rlInstance.Close()

	tty := "/dev/cu.SLAB_USBtoUART"
	serial, err := serial.OpenPort(&serial.Config{Name: tty, Baud: 115200})
	check(err)

	serIn := make(chan []byte)
	outBound := make(chan []byte)

	go func() {
		buf := make([]byte, 128)
		for {
			n, err := serial.Read(buf)
			check(err)
			if n == 0 {
				break
			}
			serIn <- buf[:n]
		}
		close(serIn)
		log.Print("serIn ends")
	}()

	go func() {
		for {
			select {
			case data := <-serIn:
				if len(data) == 0 {
					return
				}
				print(string(data))
			case data := <-outBound:
				_, err := serial.Write(data)
				check(err)
			}
		}
	}()

	outBound <- []byte("\r")
	for {
		line, err := rlInstance.Readline()
		if err != nil { // io.EOF, readline.ErrInterrupt
			break
		}
		if strings.HasPrefix(line, "include ") {
			includeFile := line[8:]
			fmt.Printf(">>> %s\n", includeFile)
			fmt.Printf("<<< %s\n", includeFile)
		} else {
			outBound <- []byte(line + "\r")
		}
	}
}
