\ core definitions

\ <<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/i2c/oled.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../flib/mecrisp/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
