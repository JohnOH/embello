# Minimal gcc makefile for LPC810 with serial output and printf

# default Linux USB device name for upload
TTY ?= /dev/ttyUSB*

# default is to include the serial output code and connect after upload
SERIAL ?= -term

# use the arm cross compiler, not std gcc
TRGT = arm-none-eabi-
CC = $(TRGT)gcc
CXX = $(TRGT)g++
CP = $(TRGT)objcopy

# compiler and linker settings
CFLAGS = -mcpu=cortex-m0plus -mthumb -I../common -Os -ggdb
CXXFLAGS = $(CFLAGS) -fno-rtti -fno-exceptions
LDFLAGS = -Wl,--script=../common/LPC810.ld -nostartfiles

# permit including this makefile from a sibling directory
vpath %.c ../common
vpath %.cpp ../common

# default target
upload: firmware.bin
	lpc21isp $(SERIAL) -control -donotstart -bin $< $(TTY) 115200 0

firmware.elf: main.o gcc_startup_lpc8xx.o
	$(CC) -o $@ $(CFLAGS) $(LDFLAGS) $^

ifneq ($(SERIAL),)
firmware.elf: printf-stdarg.o
endif

%.bin: %.elf
	$(CP) -O binary $< $@

clean:
	rm -f *.o *.elf

# these target names don't represent real files
.PHONY: upload clean
