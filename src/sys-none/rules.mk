# gcc Makefile for LPC810
# based on original file by Kamal Mostafa <kamal@whence.com>

ARCHDIR = ../arch-$(ARCH)
INCLUDES = -I$(ARCHDIR) -I$(SHARED) -I../driver -I../util -I../vendor

VPATH = $(ARCHDIR):$(SHARED):../util:../vendor

CROSS = arm-none-eabi-
CPU = -mthumb -mcpu=cortex-m0plus
WARN = -Wall
STD = -std=gnu99

CC = $(CROSS)gcc
CXX = $(CROSS)g++
LD = $(CROSS)ld
OBJCOPY = $(CROSS)objcopy
SIZE = $(CROSS)size

CFLAGS += $(CPU) $(WARN) $(STD) -MMD $(INCLUDES) \
          -Os -ffunction-sections -fno-builtin -ggdb
CXXFLAGS += $(CPU) $(WARN) -MMD $(INCLUDES) \
          -Os -ffunction-sections -fno-builtin -ggdb
CXXFLAGS += -fno-rtti -fno-exceptions

LDFLAGS += --gc-sections --library-path=$(SHARED)
LIBGCC = "$(shell $(CC) $(CFLAGS) --print-libgcc-file-name)"

OS := $(shell uname)

ifeq ($(OS), Linux)
TTY ?= /dev/ttyUSB0
endif

ifeq ($(OS), Darwin)
TTY ?= /dev/tty.usbserial-*
endif

.PHONY: all clean isp
  
all: firmware.bin firmware.hex

firmware.elf: $(ARCHDIR)/$(LINK) $(OBJS)
	@$(LD) -o $@ $(LDFLAGS) -T $(ARCHDIR)/$(LINK) $(OBJS) $(LIBGCC)
	$(SIZE) $@

clean:
	rm -f *.o *.d firmware.elf firmware.bin firmware.map firmware.hex

# this works with NXP LPC's, using serial ISP
isp: firmware.bin
	uploader $(ISPOPTS) $(TTY) firmware.bin

%.bin:%.elf
	@$(OBJCOPY) --strip-unneeded -O binary firmware.elf firmware.bin

%.hex:%.elf
	@$(OBJCOPY) --strip-unneeded -O ihex firmware.elf firmware.hex

-include $(OBJS:.o=.d)
