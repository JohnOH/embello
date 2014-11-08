# Minimal gcc makefile for LPC810

# default Linux USB device name for upload
TTY = /dev/ttyUSB*

# use the arm cross compiler, not std gcc
TRGT = arm-none-eabi-
CC = $(TRGT)gcc
CXX = $(TRGT)g++
LD = $(TRGT)gcc
CP = $(TRGT)objcopy

# compiler and linker settings
CFLAGS = -mcpu=cortex-m0plus -mthumb -I. -Os
CXXFLAGS = $(CFLAGS) -fno-rtti -fno-exceptions
LDFLAGS = -Wl,--script=LPC810.ld -nostartfiles

# default target
upload: firmware.bin
	lpc21isp -control -bin $< $(TTY) 115200 0

firmware.elf: main.o gcc_startup_lpc8xx.o
	$(LD) -o $@ $(LDFLAGS) $^

%.bin: %.elf
	$(CP) -O binary $< $@

clean:
	rm -f *.o *.elf

# these target names don't represent real files
.PHONY: upload clean
