CFLAGS += -DCORE_M0PLUS
CXXFLAGS += -DCORE_M0PLUS -I. -I../vendor/lpcopen/inc
ISPOPTS += -s
LINK = LPC810.ld
ARCH = lpc8xx

OBJS = rf_ping.o system_LPC8xx.o gcc_startup_lpc8xx.o \
		uart.o printf.o printf-retarget.o

default: isp

SHARED = ../sys-none
include $(SHARED)/rules.mk
