package main

import (
	"bufio"
	"bytes"
	"flag"
	"fmt"
	"io/ioutil"
	"log"
	"os"
	"strings"
	"time"

	"github.com/tarm/serial"
	"gopkg.in/readline.v1"
)

var (
	rlInstance *readline.Instance
	conn       *serial.Port
	serIn      = make(chan []byte)
	outBound   = make(chan string)
	progress   = make(chan bool, 1)
	incLevel   = make(chan int)

	port   = flag.String("p", "", "serial port (required: /dev/tty*, COM*, etc)")
	baud   = flag.Int("b", 115200, "baud rate")
	upload = flag.String("u", "", "upload the specified firmware, then quit")
	expand = flag.String("e", "", "expand specified file to stdout, then quit")
)

func main() {
	flag.Parse()
	var err error

	// expansion does not use the serial port, it just expands include lines
	if *expand != "" {
		expandFile(*expand)
		return
	}

	fmt.Println("Connecting to", *port)
	if *port == "" {
		flag.PrintDefaults()
		os.Exit(1)
	}
	config := serial.Config{Name: *port, Baud: *baud}
	if *upload != "" {
		config.Parity = serial.ParityEven
	}
	conn, err = serial.OpenPort(&config)
	check(err)
	//defer conn.Close()

	go serialInput() // feed the serIn channel

	if *upload != "" {
		f, err := os.Open(*upload)
		if err != nil {
			fmt.Println(err)
		}
		defer f.Close()

		data, err := ioutil.ReadAll(f)
		check(err)

		fmt.Println("Uploading", len(data), "bytes")
		uploadSTM32(data)
		return
	}

	rlInstance, err = readline.NewEx(&readline.Config{
		UniqueEditLine: true,
	})
	check(err)
	defer rlInstance.Close()

	go serialExchange()

	outBound <- ""
	<-progress
	for {
		line, err := rlInstance.Readline()
		if err != nil { // io.EOF, readline.ErrInterrupt
			break
		}
		parseAndSend(line)
	}
}

func parseAndSend(line string) {
	if strings.HasPrefix(line, "include ") {
		doInclude(line[8:])
	} else {
		outBound <- line
		<-progress
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
	for {
		buf := make([]byte, 100)
		n, err := conn.Read(buf)
		check(err)
		if n == 0 {
			close(serIn)
			return
		}
		serIn <- buf[:n]
	}
}

func readWithTimeout() []byte {
	select {
	case data := <-serIn:
		return data
	case <-time.After(500 * time.Millisecond):
		return nil
	}
}

func serialExchange() {
	includeDepth := 0
	for {
		select {
		case data := <-serIn:
			if len(data) == 0 {
				return
			}
			fmt.Print(string(data))
		case line := <-outBound:
			including := includeDepth > 0
			// the task here is to omit "normal" output for included lines,
			// i.e. lines which only generate an echo, a space, and " ok.\n"
			// everything else should be echoed in full, including the input
			if len(line) > 0 {
				serialSend(line)
				prefix, matched := expectEcho(line, func(s string) {
					fmt.Print(s) // called to flush pending serial input lines
				})
				fmt.Print(prefix)
				if matched && !including {
					fmt.Print(line)
					line = ""
				}
			}
			// now that the echo is done, send a CR and wait for the prompt
			serialSend("\r")
			prompt := " ok.\n"
			prefix, matched := expectEcho(prompt, func(s string) {
				fmt.Print(line + s) // show original command first
				line = ""
			})
			if !matched {
				prompt = ""
			}
			if !including || prefix != " " || !matched {
				fmt.Print(line + prefix + prompt)
			}
			// signal to sender that this request has been processed
			progress <- matched
		case n := <-incLevel:
			includeDepth += n
		}
	}
}

func expectEcho(match string, flusher func(string)) (string, bool) {
	var collected []byte
	for {
		data := readWithTimeout()
		if len(data) == 0 {
			return string(collected), false
		}
		collected = append(collected, data...)
		if bytes.HasSuffix(collected, []byte(match)) {
			bytesBefore := len(collected) - len(match)
			return string(collected[:bytesBefore]), true
		}
		if n := bytes.LastIndexByte(collected, '\n'); n >= 0 {
			flusher(string(collected[:n+1]))
			collected = collected[n+1:]
		}
	}
}

func serialSend(data string) {
	_, err := conn.Write([]byte(data))
	check(err)
}

func doInclude(fname string) {
	incLevel <- +1
	defer func() { incLevel <- -1 }()

	lineNum := 0
	fmt.Printf("\\\t>>> include %s\n", fname)
	defer func() {
		fmt.Printf("\\\t<<<<<<<<<<< %s (%d lines)\n", fname, lineNum)
	}()

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
			if len(*expand) == 0 {
				continue // don't send empty or comment-only lines
			}
		}

		parseAndSend(line)
	}
}

func expandFile(fname string) {
	go func() {
		for line := range outBound {
			fmt.Println(line)
			progress <- true
		}
	}()

	go func() {
		for range incLevel {}
	}()

	doInclude(fname)
}
