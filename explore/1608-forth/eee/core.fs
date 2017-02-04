\ core definitions

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/mecrisp/multi.fs
include ../flib/mecrisp/disassembler-m3.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
