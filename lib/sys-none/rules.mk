# gcc Makefile for LPC810
# based on original file by Kamal Mostafa <kamal@whence.com>

LIBDIR = $(dir $(SHARED))
LINKWITH = GCC
#LINKWITH = LD

ARCHDIR = $(LIBDIR)arch-$(ARCH)
INCLUDES = -I$(ARCHDIR) -I$(SHARED) -I$(LIBDIR)driver -I$(LIBDIR)util \
					 -I$(LIBDIR)vendor -I$(LIBDIR)vendor/lpcopen/inc

# Output directory and files
BUILDDIR = build
OUTFILES = $(BUILDDIR)/firmware.elf \
           $(BUILDDIR)/firmware.hex \
           $(BUILDDIR)/firmware.bin
OBJDIR    = $(BUILDDIR)/obj

VPATH = $(ARCHDIR):$(SHARED):$(LIBDIR)util:$(LIBDIR)vendor

CROSS = arm-none-eabi-
CPU = -mthumb -mcpu=cortex-m0plus -DCORE_M0PLUS
WARN = -Wall
STD = -std=gnu99

CC = $(CROSS)gcc
CXX = $(CROSS)g++
LD = $(CROSS)ld
OBJCOPY = $(CROSS)objcopy
SIZE = $(CROSS)size

OBJCTS= $(OBJS:%.o=$(OBJDIR)/%.o)

CFLAGS += $(CPU) $(WARN) $(STD) -MMD $(INCLUDES) \
          -Os -ffunction-sections -fno-builtin -ggdb
CXXFLAGS += $(CPU) $(WARN) -MMD $(INCLUDES) \
          -Os -ffunction-sections -fno-builtin -ggdb

#needed to shrink size compared to previous
CXXFLAGS += -fno-rtti -fno-exceptions

ifeq ($(LINKWITH), GCC)
LDFLAGS += -Wl,--script=$(ARCHDIR)/$(LINK) -Wl,--gc-sections -nostartfiles -L$(SHARED)
else
LDFLAGS += --gc-sections --library-path=$(SHARED)
endif

LIBGCC = "$(shell $(CC) $(CFLAGS) --print-libgcc-file-name)"

OS := $(shell uname)

ifeq ($(OS), Linux)
TTY ?= /dev/ttyUSB0
endif

ifeq ($(OS), Darwin)
TTY ?= /dev/tty.usbserial-*
endif

.PHONY: all clean isp

all: $(OUTFILES)

$(BUILDDIR) $(OBJDIR):
	mkdir -p $(OBJDIR)

$(OBJDIR)/%.o: %.c
	$(CC) $(CFLAGS) -c -o $@ $<

$(OBJDIR)/%.o: %.cpp
	$(CXX) $(CXXFLAGS) -c -o $@ $<

$(OBJCTS): | $(OBJDIR)

ifeq ($(LINKWITH), GCC)
%.elf: $(OBJCTS)
	$(CC) -o $@ $(CFLAGS) $(LDFLAGS) $^
	$(SIZE) $@
else
%.elf: $(OBJCTS)
	$(LD) -o $@ $(LDFLAGS) -T $(ARCHDIR)/$(LINK) $(OBJCTS) $(LIBGCC)
	$(SIZE) $@
endif

clean:
	rm -fR $(BUILDDIR)

# this works with NXP LPC's, using serial ISP
isp: $(BUILDDIR)/firmware.bin
	uploader $(ISPOPTS) $(TTY) $^

%.bin: %.elf
	@$(OBJCOPY) --strip-unneeded -O binary $^ $@

%.hex: %.elf
	@$(OBJCOPY) --strip-unneeded -O ihex $^ $@

-include $(OBJCTS:.o=.d)
