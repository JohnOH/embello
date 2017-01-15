package main

//go:generate ./h2go.sh

import (
	"fmt"
	"os"

	"github.com/jeelabs/embello/tools/fontconv/fonts"
)

func main() {
	os.MkdirAll("fonts-show", 0755)
	os.MkdirAll("fonts-raw", 0755)

	for k, v := range fonts.All {
		if isMonospaced(k) {
			fmt.Printf("%5d  %s\n", len(v), k)

			fshow := "fonts-show/" + k + ".txt"
			if fp, err := os.Create(fshow); err == nil {
				os.Stdout = fp
				showFont(k)
				os.Stdout = os.Stderr
				fp.Close()
			}
		}

		fraw := "fonts-raw/" + k + ".bin"
		if fp, err := os.Create(fraw); err == nil {
			for _, b := range v {
				fp.Write([]byte{byte(b)})
			}
			fp.Close()
		}
	}
}

func showFont(name string) {
	font := fonts.All[name]
	height := int(font[0])
	width := int(font[1])
	off := 2 + height*width
	first := int(font[off])
	count := int(font[off+1])
	hpix := int(font[off+2])
	vpix := int(font[off+3])

	fmt.Println(len(font), height, width, first, count, hpix, vpix)

	printBar(hpix)
	for i := 0; i < count; i++ {
		glyph(hpix, i*hpix, height, width, font[2:2+off])
		printBar(hpix)
	}
}

func printBar(n int) {
	fmt.Print("+")
	for col := 0; col < n; col++ {
		fmt.Print("-")
	}
	fmt.Println("+")
}

func isMonospaced(name string) bool {
	font := fonts.All[name]
	height := int(font[0])
	width := int(font[1])
	off := 2 + height*width
	return font[off+2] != 0
}

func glyph(n, p, h, w int, f []int16) {
	for row := 0; row < h; row++ {
		fmt.Print("|")
		for col := 0; col < n; col++ {
			c := " "
			k := row*w + (p+col)/8
			if ((f[k] >> uint((p+col)%8)) & 1) != 0 {
				c = "#"
			}
			fmt.Print(c)
		}
		fmt.Println("|")
	}
}
