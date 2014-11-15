# This makefile reuses the minimal one, but runs it in a sibling directory

VPATH = ../minimal
include $(VPATH)/Makefile

# extend the CFLAGS definition and replace LDFLAGS with a different one
CFLAGS += -I$(VPATH)
LDFLAGS = -Wl,--script=$(VPATH)/LPC810.ld -nostartfiles

sine.h: gensine.go
	go run $< >$@

main.o: sine.h
