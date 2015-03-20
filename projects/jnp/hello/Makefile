CXXFLAGS += -I.
ISPOPTS += -s
LINK = LPC824.ld
ARCH = lpc8xx

OBJS = main.o system_LPC8xx.o gcc_startup_lpc8xx.o \
		uart.o printf.o printf-retarget.o

default: isp

LIBDIR = ../../../lib
SHARED = $(LIBDIR)/sys-none
include $(SHARED)/rules.mk
