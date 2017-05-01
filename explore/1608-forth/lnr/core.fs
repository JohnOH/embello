\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/any/varint.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
