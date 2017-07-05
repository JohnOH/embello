\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here hex.

PC13 constant LED

include ../flib/spi/rf69.fs
include ../flib/mecrisp/multi.fs
include ../flib/any/timed.fs

cornerstone <<<core>>>
hello
