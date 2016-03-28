package main

import (
	"fmt"
	"bufio"
	"log"
	"os"
	"strings"

	"github.com/tarm/serial"
	"gopkg.in/readline.v1"
)

var (
	rlInstance *readline.Instance
	conn       *serial.Port
	serIn      = make(chan []byte)
	outBound   = make(chan []byte)
	incLevel   = make(chan int)
)

func main() {
	var err error
	rlInstance, err = readline.NewEx(&readline.Config{
		UniqueEditLine: true,
	})
	check(err)
	defer rlInstance.Close()

	tty := "/dev/cu.SLAB_USBtoUART"
	conn, err = serial.OpenPort(&serial.Config{Name: tty, Baud: 115200})
	check(err)

	go serialInput()
	go serialExchange()

	outBound <- []byte("\r")
	for {
		line, err := rlInstance.Readline()
		if err != nil { // io.EOF, readline.ErrInterrupt
			break
		}
		if strings.HasPrefix(line, "include ") {
			doInclude(line[8:])
		} else {
			outBound <- []byte(line + "\r")
		}
	}
}

func check(err error) {
	if err != nil {
		if rlInstance != nil {
			rlInstance.Close()
		}
		log.Fatal(err)
	}
}

func serialInput() {
	buf := make([]byte, 128)
	for {
		n, err := conn.Read(buf)
		check(err)
		if n == 0 {
			close(serIn)
			return
		}
		serIn <- buf[:n]
	}
}

func serialExchange() {
	level := 0
	for {
		select {
		case data := <-serIn:
			if len(data) == 0 {
				return
			}
			print(string(data))
		case data := <-outBound:
			_, err := conn.Write(data)
			check(err)
		case n := <-incLevel:
			level += n
		}
	}
}

func doInclude(fname string) {
	incLevel <- +1
	defer func() { incLevel <- -1 }()

	lineNum := 0
	fmt.Printf(">>> %s\n", fname)
	defer fmt.Printf("<<< %s (%d lines)\n", fname, lineNum)

	f, err := os.Open(fname)
	if err != nil {
		fmt.Println(err)
	}
	defer f.Close()

	scanner := bufio.NewScanner(f)
	for scanner.Scan() {
		line := scanner.Text()
		lineNum++

		s := strings.TrimLeft(line, " ")
		if s == "" || strings.HasPrefix(s, "\\ ") {
			continue // don't send empty or comment-only lines
		}

		outBound <- []byte(line + "\r")
	}
}
