LINK = LPC824.ld
ARCH = lpc8xx

APP_OBJS ?= $(patsubst %.c,%.o,$(wildcard *.c)) \
						$(patsubst %.cpp,%.o,$(wildcard *.cpp))

OBJS = $(APP_OBJS) embello.o system_LPC8xx.o gcc_startup_lpc8xx.o \
       uart.o printf.o printf-retarget.o

.DEFAULT_GOAL = isp

include $(SHARED)/rules.mk
