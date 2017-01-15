CFLAGS += -DCORE_M0PLUS
CXXFLAGS += -DCORE_M0PLUS -I. -I../../../lib/vendor/lpcopen/inc
ISPOPTS += -s -i 5
LINK = LPC812.ld
ARCH = lpc8xx

OBJS = main.o embello.o system_LPC8xx.o gcc_startup_lpc8xx.o \
		printf.o printf-retarget.o

default: isp

SHARED = ../../../lib/sys-none
include $(SHARED)/rules.mk
