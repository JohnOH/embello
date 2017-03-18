package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"os"
)

const (
	PERLINE = 32
	FROM    = "disk.img"
	TO      = "disk.fs"
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

	fmt.Fprintln(fout, "$00 >mb  $0000 a  hex")
	for i := 0; i < len(data); i += PERLINE {
		if i%1024 == 0 {
			fmt.Fprint(fout, "p . ")
		} else {
			fmt.Fprint(fout, "    ")
		}
		for j := 0; j < PERLINE; j++ {
			fmt.Fprintf(fout, "%02X", data[i+j])
			if j%4 == 3 {
				fmt.Fprint(fout, " ")
			}
		}
		fmt.Fprintln(fout, "w32")
	}
	fmt.Fprintf(fout, "p .  decimal  $6000 a  \\ $%04X bytes\n", len(data))
}
