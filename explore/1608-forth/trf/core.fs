\ core definitions

\ <<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/mecrisp/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
