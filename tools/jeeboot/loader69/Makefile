CFLAGS += -DCORE_M0PLUS -DSTARTUP_NO_IRQS
CXXFLAGS += -DCORE_M0PLUS -I. -I../../../lib/vendor/lpcopen/inc -I../include
ISPOPTS += -w -s -i 5
LINK = LPC824-28k.ld
ARCH = lpc8xx

OBJS = main.o embello.o system_LPC8xx.o gcc_startup_lpc8xx.o \
		uart.o printf.o printf-retarget.o

default: all
	uploader -w -o 28672 $(TTY) build/firmware.bin
	uploader -s $(TTY) build/firmware.bin

LIBDIR = ../../../lib
SHARED = $(LIBDIR)/sys-none
include $(SHARED)/rules.mk
