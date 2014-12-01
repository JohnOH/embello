# This makefile reuses the minimal one, but runs it in a sibling directory
include ../minimal/Makefile

sine.h: gensine.go
	go run $< >$@

main.o: sine.h
