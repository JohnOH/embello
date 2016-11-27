\ core definitions

\ <<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/i2c/oled.fs

include ../mlib/graphics.fs
include ../flib/any/digits.fs
include ../mlib/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
