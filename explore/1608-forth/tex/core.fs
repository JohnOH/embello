\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/any/varint.fs
include ../flib/spi/smem.fs
include ../flib/i2c/ssd1306.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../flib/mecrisp/multi.fs

include serplus.fs
include x-serplus.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
