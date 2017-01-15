\ core definitions

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/i2c/oled.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs

\ tht specifics (modified multi.fs!)
include ../flib/mecrisp/multi.fs
include lib/timed.fs
include lib/pid.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
