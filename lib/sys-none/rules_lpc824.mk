LINK = LPC824.ld
ARCH = lpc8xx

OBJS = $(APPOBJ) embello.o system_LPC8xx.o gcc_startup_lpc8xx.o \
	uart.o printf.o printf-retarget.o

include $(SHARED)/rules.mk
