hello.fs: hello.asm
	z80asm -o hello.bin hello.asm
	xxd -i hello.bin | \
	  sed -e 's/0x/$$/g' -e 's/,/ w/g' -e 's/0$$/0 w/' -e '1d' -e '$$d' | \
	    sed '$$d' >hello.fs && rm hello.bin
