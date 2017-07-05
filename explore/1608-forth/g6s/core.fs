\ core definitions

<<<board>>>
compiletoflash
( core start: ) here hex.

include ../flib/i2c/ssd1306.fs
include ../flib/mecrisp/graphics.fs
include ../flib/any/digits.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

cornerstone <<<core>>>
hello
