package main

import (
	"bufio"
	"bytes"
	"encoding/hex"
	"flag"
	"fmt"
	"hash/crc32"
	"io"
	"io/ioutil"
	"log"
	"net"
	"os"
	"os/exec"
	"strconv"
	"strings"
	"time"

	"github.com/tarm/serial"
	"gopkg.in/readline.v1"
)

var (
	rlInstance *readline.Instance
	conn       io.ReadWriter
	serIn      = make(chan []byte)
	outBound   = make(chan string)
	progress   = make(chan bool, 1)
	incLevel   = make(chan int)

	port    = flag.String("p", "", "serial port (required: /dev/tty* or COM*)")
	exe     = flag.Bool("x", false, "executable (consumes all remaining args)")
	baud    = flag.Int("b", 115200, "baud rate")
	upload  = flag.String("u", "", "upload the specified firmware, then quit")
	expand  = flag.String("e", "", "expand specified file to stdout, then quit")
	verbose = flag.Bool("v", false, "verbose output, for debugging only")
	capture = flag.String("c", "", "a file where captured output is appended")
	timeout = flag.Duration("t", 500*time.Millisecond, "serial echo timeout")
)

func main() {
	flag.Parse()
	var err error

	// expansion does not use the serial port, it just expands include lines
	if *expand != "" {
		expandFile()
		return
	}

	if *exe && flag.NArg() > 0 {
		conn, err = launch(flag.Args())
		*port = "(" + strings.Join(flag.Args(), " ") + ")"
	} else if *port != "" && flag.NArg() == 0 {
		conn, err = connect(*port)
	} else {
		flag.PrintDefaults()
		os.Exit(1)
	}
	check(err)
	//defer conn.Close()
	fmt.Println("Connected to:", *port)

	go serialInput() // feed the serIn channel

	// firmware upload uses serial in a different way, needs to quit when done
	if *upload != "" {
		firmwareUpload()
		return
	}

	rlInstance, err = readline.NewEx(&readline.Config{
		UniqueEditLine: true,
	})
	check(err)
	defer rlInstance.Close()

	go serialExchange()

	//conn.Flush()
	readWithTimeout() // get rid of partial pending data
	for {
		line, err := rlInstance.Readline()
		if err != nil { // io.EOF, readline.ErrInterrupt
			break
		}
		parseAndSend(line)
		if strings.HasPrefix(line, "include ") {
			fmt.Println("\\ done.")
		}
	}
}

// isNetPort returns true if the argument is of the form "...:<N>"
func isNetPort(s string) bool {
	if n := strings.Index(s, ":"); n > 0 {
		p, e := strconv.Atoi(s[n+1:])
		return e == nil && p > 0
	}
	return false
}

// connect to either a net port for telnet use, or to a serial device
func connect(name string) (io.ReadWriter, error) {
	if isNetPort(name) {
		return net.Dial("tcp", name)
	}

	config := serial.Config{Name: *port, Baud: *baud}
	if *upload != "" {
		config.Parity = serial.ParityEven
	}
	return serial.OpenPort(&config)
}

// launch an executable and talk to it via pipes
func launch(args []string) (io.ReadWriter, error) {
	cmd := exec.Command(args[0], args[1:]...)
	var cmdio exePipe
	var err error
	cmdio.to, err = cmd.StdinPipe()
	if err == nil {
		cmdio.from, err = cmd.StdoutPipe()
		if err == nil {
			err = cmd.Start()
		}
	}
	return &cmdio, err
}

type exePipe struct {
	from io.Reader
	to   io.Writer
}

func (e *exePipe) Read(p []byte) (n int, err error) {
	return e.from.Read(p)
}

func (e *exePipe) Write(p []byte) (n int, err error) {
	return e.to.Write(p)
}

func parseAndSend(line string) {
	if strings.HasPrefix(line, "include ") {
		for _, fname := range strings.Split(line[8:], " ") {
			doInclude(fname)
		}
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
	var f *os.File
	if *capture != "" {
		var err error
		opts := os.O_WRONLY | os.O_APPEND | os.O_CREATE
		f, err = os.OpenFile(*capture, opts, 0666)
		check(err)
		defer f.Close()
	}
	for {
		buf := make([]byte, 600)
		n, err := conn.Read(buf)
		if n == 0 {
			fmt.Print(" [disconnected] ")
			time.Sleep(2 * time.Second)
			conn, err = connect(*port)
			check(err)
			fmt.Println("[reconnected]")
			continue
		}
		check(err)
		if f != nil {
			f.Write(buf[:n])
		}
		serIn <- buf[:n]
	}
}

func readWithTimeout() []byte {
	select {
	case data := <-serIn:
		return data
	case <-time.After(*timeout):
		return nil
	}
}

func serialExchange() {
	depth := 0
	for {
		select {

		case data := <-serIn:
			if len(data) == 0 {
				return
			}
			fmt.Print(string(data))

		case line := <-outBound:
			immediate := depth == 0
			// the task here is to omit "normal" output for included lines,
			// i.e. lines which only generate an echo, a space, and " ok.\n"
			// everything else should be echoed in full, including the input
			if len(line) > 0 {
				serialSend(line)
				prefix, matched := expectEcho(line, false, func(s string) {
					fmt.Print(s) // called to flush pending serial input lines
				})
				fmt.Print(prefix)
				if matched && immediate {
					fmt.Print(line)
					line = ""
				}
			}
			// now that the echo is done, send a CR and wait for the prompt
			serialSend("\r")
			prompt := " ok.\n"
			prefix, matched := expectEcho(prompt, immediate, func(s string) {
				fmt.Print(line + s) // show original command first
				line = ""
			})
			if !matched {
				prompt = ""
			}
			if immediate || prefix != " " || !matched {
				fmt.Print(line + prefix + prompt)
			}
			// signal to sender that this request has been processed
			progress <- matched

		case n := <-incLevel:
			depth += n
		}
	}
}

func expectEcho(match string, immed bool, flusher func(string)) (string, bool) {
	var collected []byte
	for {
		data := readWithTimeout()
		collected = append(collected, data...)
		if bytes.HasSuffix(collected, []byte(match)) {
			bytesBefore := len(collected) - len(match)
			return string(collected[:bytesBefore]), true
		}
		if immed || len(data) == 0 {
			return string(collected), false
		}
		if n := bytes.LastIndexByte(collected, '\n'); n >= 0 {
			flusher(string(collected[:n+1]))
			collected = collected[n+1:]
		}
	}
}

func serialSend(data string) {
	for len(data) > 0 {
		t := data
		// send in chunks under 64 bytes to simplify USB-serial use
		if len(t) > 60 {
			t = t[:60]
		}
		data = data[len(t):]

		_, err := conn.Write([]byte(t))
		check(err)

		// when chunked, add a very brief delay to force separate sends
		if len(data) > 0 {
			time.Sleep(2 * time.Millisecond)
		}
	}
}

func doInclude(fname string) {
	if fname == "" {
		return // silently ignore empty files
	}

	incLevel <- +1
	defer func() { incLevel <- -1 }()

	lineNum := 0
	fmt.Printf("\\       >>> include %s\n", fname)
	defer func() {
		fmt.Printf("\\       <<<<<<<<<<< %s (%d lines)\n", fname, lineNum)
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

func expandFile() {
	go func() {
		for line := range outBound {
			fmt.Println(line)
			progress <- true
		}
	}()

	go func() {
		for range incLevel {
		}
	}()

	for _, fname := range strings.Split(*expand, ",") {
		doInclude(fname)
	}
}

func hexToBin(data []byte) []byte {
	var bin []byte
	for _, line := range strings.Split(string(data), "\n") {
		if strings.HasSuffix(line, "\r") {
			line = line[:len(line)-1]
		}
		if len(line) == 0 {
			continue
		}
		if line[0] != ':' || len(line) < 11 {
			fmt.Println("Not ihex format:", line)
			os.Exit(1)
		}
		bytes, err := hex.DecodeString(line[1:])
		check(err)
		if bytes[3] != 0x00 {
			continue
		}
		offset := (int(bytes[1]) << 8) + int(bytes[2])
		length := bytes[0]
		for offset > len(bin) {
			bin = append(bin, 0xFF)
		}
		bin = append(bin, bytes[4:4+length]...)
	}
	return bin
}

func firmwareUpload() {
	f, err := os.Open(*upload)
	if err != nil {
		fmt.Println(err)
	}
	defer f.Close()

	data, err := ioutil.ReadAll(f)
	check(err)

	// convert to binary if first bytes look like they are in hex format
	tag := ""
	if len(data) > 11 && data[0] == ':' {
		_, err = hex.DecodeString(string(data[1:11]))
		if err == nil {
			data = hexToBin(data)
			tag = " (converted from Intel HEX)"
		}
	}

	fmt.Printf("        File: %s\n", *upload)
	fmt.Printf("       Count: %d bytes%s\n", len(data), tag)
	fmt.Printf("    Checksum: %08x hex\n", crc32.ChecksumIEEE(data))

	uploadSTM32(data)
}
