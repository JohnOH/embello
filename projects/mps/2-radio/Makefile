CFLAGS += -DCORE_M0PLUS
CXXFLAGS += -DCORE_M0PLUS -I. -I../../../lib/vendor/lpcopen/inc
ISPOPTS += -w
LINK = LPC810.ld
ARCH = lpc8xx

OBJS = main.o system_LPC8xx.o gcc_startup_lpc8xx.o

default: isp

SHARED = ../../../lib/sys-none
include $(SHARED)/rules.mk
