subst = -e 's/0x/$$/g' -e 's/,/ w/g' -e 's/[^w]$$/& w/' -e '1d' -e '$$d'

all: hello.fs hellow.fs flash.fs powerup.bin

hello.fs: hello.asm
	z80asm -l -o hello.bin hello.asm 2>hello.lst
	@xxd -i hello.bin | sed $(subst) | sed '$$d' >$@ && rm hello.bin

hellow.fs: hellow.asm
	z80asm -l -o hellow.bin hellow.asm 2>hellow.lst
	@xxd -i hellow.bin | sed $(subst) | sed '$$d' >$@ && rm hellow.bin

flash.fs: flash.asm
	z80asm -l -o flash.bin flash.asm 2>flash.lst
	@xxd -i flash.bin | sed $(subst) | sed '$$d' >$@ && rm flash.bin

powerup.bin: powerup.asm
	z80asm -l -o powerup.bin powerup.asm 2>powerup.lst
