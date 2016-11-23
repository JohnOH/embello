\ core definitions

\ <<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/spi/smem.fs
include ../flib/i2c/oled.fs

include ../mlib/graphics.fs
include ../flib/any/digits.fs
include ../mlib/multi.fs

include serplus.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
