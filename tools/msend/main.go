// This utility is intended for "picocom" in combination with Mecrisp Forth.
// It sends out text lines from a file without over-running the input buffer.
// Relies on the fact that processing starts after a newline has been sent.
//
// Msend does this by waiting up to 500 milliseconds for new input to arrive.
// The trick is that as soon as a received text line matches what was just
// sent out plus an "ok." prompt at the end, then it immediately moves on to
// the next line. This allows sending source code lines at maximum speed.
//
// include <filename>
//		Include directives can be used to insert another source file.
//
// require <filename>
//		Similar to include, but this won't re-include a file if already sent.
//
// To reduce clutter, the exact-echo lines are also not passed on to picocom.
// Only lines which are not precisely the same as the input will be shown.
// Comment lines starting with "\" and empty lines are not sent.
//
// If there's a "not found" error, it will be shown and abort the upload.
//
// -jcw, 2016-02-18

package main

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"strings"
	"time"
)

var serIn = make(chan string)
var filesSeen = make(map[string]struct{})

func main() {
	log.SetFlags(0) // no date & time

	if len(os.Args) < 2 {
		log.Fatal("Usage: msend files...")
	}

	// feed incoming lines into a channel, so we can timeout on it
	go func() {
		scanner := bufio.NewScanner(os.Stdin)
		for scanner.Scan() {
			serIn <- scanner.Text()
		}
		close(serIn)
	}()

	defer func() {
		if e := recover(); e != nil {
			fmt.Fprintf(os.Stderr, "\n")
			log.Fatal(e)
		}
	}()

	for _, fn := range os.Args[1:] {
		send(fn)
	}
}

func send(fname string) {
	f, err := os.Open(fname)
	if err != nil {
		log.Fatal(err)
	}
	defer f.Close()
	filesSeen[fname] = struct{}{}

	// main loop, driven by the text lines present in the source code
	scanner := bufio.NewScanner(f)
	lineNum := 0
	defer func() {
		fmt.Fprintf(os.Stderr, "\t<<<<<<<<<<< %s (%d lines)\n", fname, lineNum)
	}()

	for scanner.Scan() {
		line := scanner.Text()
		lineNum++

		if line == "" || strings.HasPrefix(line, "\\ ") {
			continue // don't send empty or comment-only lines
		}

		incl := strings.HasPrefix(line, "include ")
		if incl || strings.HasPrefix(line, "require ") {
			fn := strings.Split(line, " ")[1]
			if _, ok := filesSeen[fn]; incl || !ok {
				fmt.Fprintf(os.Stderr, "\t>>> %s\n", line)
				send(fn) // recurse
			} else {
				fmt.Fprintf(os.Stderr, "\t=== #require %s (skipped)\n", fn)
			}
			continue
		}

		fmt.Println(line)
	L: // if input has been received but did not match, we loop here
		select {

		// wait for incoming lines from the serial port
		case s := <-serIn:
			if s != line+"  ok." {
				if strings.HasSuffix(s, "not found.") {
					panic(s)
				}
				fmt.Fprintln(os.Stderr, s)
				goto L
			}

		// continue sending if no matching input was found within 500 ms
		case <-time.After(500 * time.Millisecond):
		}
	}
}
