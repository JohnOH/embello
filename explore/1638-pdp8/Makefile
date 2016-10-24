all: p8 welcome.bin
	./p8 welcome.bin

p8: p8.c
	gcc -o p8 -std=c99 p8.c

welcome.bin: pal welcome.pal
	./pal welcome.pal

pal: pal.c
	gcc -o pal -w -Wno-return-type pal.c

focal: p8
	@echo 'Starting FOCAL, hit ctrl-\ to quit...'
	./p8 focal.bin

clean:
	rm -f pal p8 welcome.bin welcome.lst
