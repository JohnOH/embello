\ Tiny RF node

\ define some missing constants
4 constant io-ports  \ A..D
RCC $18 + constant RCC-APB2ENR

include ../mlib/hexdump.fs
include ../flib/io-stm32f1.fs
include ../flib/pins48.fs
include ../flib/spi-stm32f1.fs
include ../flib/rf69.fs

6 rf69.group !
rf69-listen
