\ core definitions

\ <<<board>>>
cr compiletoflash
( core start: ) here dup hex.

9 constant I2C.DELAY
include ../flib/any/i2c-bb.fs

include ../flib/i2c/oled.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../flib/mecrisp/multi.fs
include ../flib/mecrisp/quotation.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
