\ core definitions

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/i2c/oled.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs

\ tht specifics (modified multi.fs!)
include ../flib/mecrisp/multi.fs

include ../flib/any/timed.fs
include ../flib/any/pid.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
