\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/spi/rf69.fs
include ../flib/any/varint.fs
include ../flib/i2c/mcp3424.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
