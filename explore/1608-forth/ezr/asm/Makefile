all: hello.fs hellow.fs

hello.fs: hello.asm
	z80asm -o hello.bin hello.asm
	xxd -i hello.bin | \
	  sed -e 's/0x/$$/g' -e 's/,/ w/g' -e 's/0$$/0 w/' -e '1d' -e '$$d' | \
	    sed '$$d' >hello.fs && rm hello.bin

hellow.fs: hellow.asm
	z80asm -o hellow.bin hellow.asm
	xxd -i hellow.bin | \
	  sed -e 's/0x/$$/g' -e 's/,/ w/g' -e 's/0$$/0 w/' -e '1d' -e '$$d' | \
	    sed '$$d' >hellow.fs && rm hellow.bin
