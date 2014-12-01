# This makefile reuses the serial one, but runs it in a sibling directory

vpath %.c ../serial
vpath %.cpp ../serial

include ../serial/Makefile

# extend the CFLAGS definition and replace LDFLAGS with a different one
CFLAGS += -I../serial
LDFLAGS = -Wl,--script=../serial/LPC810.ld -nostartfiles
