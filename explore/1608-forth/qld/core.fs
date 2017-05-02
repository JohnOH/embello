\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here dup hex.

9 constant I2C.DELAY
include ../flib/any/i2c-bb.fs

include ../flib/i2c/ssd1306.fs
include ../flib/mecrisp/graphics.fs
include ../flib/mecrisp/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
