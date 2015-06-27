LIBDIR = ../../../lib

CFLAGS += -DCORE_M0PLUS -I$(LIBDIR)/vendor/lpcopen/inc
CXXFLAGS += -DCORE_M0PLUS -I. -I$(LIBDIR)/vendor/lpcopen/inc
ISPOPTS += -s
LINK = LPC810.ld
ARCH = lpc8xx

OBJS = main.o embello.o system_LPC8xx.o gcc_startup_lpc8xx.o \
		uart.o printf.o printf-retarget.o

default: isp

SHARED = $(LIBDIR)/sys-none
include $(SHARED)/rules.mk
