\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/spi/sdcard.fs
include ../flib/stm32f1/spi2.fs
include ../flib/stm32f1/uart2.fs
include ../flib/stm32f1/uart2-irq.fs

\ 9 constant I2C.DELAY
\ include ../flib/any/i2c-bb.fs

include ../flib/mecrisp/quotation.fs
include ../flib/mecrisp/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
