BINARY = usbserup

OPENCM3_DIR=./libopencm3
LDSCRIPT = ./stm32f100xb-high.ld
CFLAGS = -std=gnu99

default: all data.h
	arm-none-eabi-size usbserup.elf

data.h: bin2h $(BINARY).bin
	./bin2h $(BINARY).bin >$@

bin2h: bin2h.c
	gcc -o $@ $<

include ./libopencm3.target.mk
