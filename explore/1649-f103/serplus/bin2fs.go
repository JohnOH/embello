package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"os"
)

const (
	PERLINE = 16
	FROM    = "serplus.bin"
	TO      = "serplus.fs"
)

func main() {
	fin, err := os.Open(FROM)
	if err != nil {
		log.Fatal(err)
	}
	defer fin.Close()

	fout, err := os.Create(TO)
	if err != nil {
		log.Fatal(err)
	}
	defer fout.Close()

	data, err := ioutil.ReadAll(fin)
	if err != nil {
		log.Fatal(err)
	}

	origLen := len(data)
	for len(data)%PERLINE != 0 {
		data = append(data, 0xFF)
	}
	fmt.Printf("  %s: %d => %d bytes\n", TO, origLen, len(data))

	fmt.Fprintln(fout, "create SERPLUS.DATA")
	for i := 0; i < len(data); i += PERLINE {
		for j := 0; j < PERLINE; j++ {
			switch j {
			case 0:
				fmt.Fprint(fout, "  $")
			case 4, 8, 12:
				fmt.Fprint(fout, " , $")
			}
			fmt.Fprintf(fout, "%02X", data[(i+j)^3])
		}
		fmt.Fprintln(fout, " ,")
	}
	fmt.Fprintf(fout, "%d constant SERPLUS.SIZE\n", len(data))
}
