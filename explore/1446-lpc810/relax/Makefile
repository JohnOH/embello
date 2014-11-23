# This makefile reuses the minimal one, but runs it in a sibling directory

vpath %.c ../minimal
vpath %.cpp ../minimal

include ../minimal/Makefile

# extend the CFLAGS definition and replace LDFLAGS with a different one
CFLAGS += -I../minimal
LDFLAGS = -Wl,--script=../minimal/LPC810.ld -nostartfiles
