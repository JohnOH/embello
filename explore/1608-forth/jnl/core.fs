\ core libraries

cr <<<board>>>
cr
compiletoflash
( core start: ) here dup hex.

include ../flib/rf69.fs
include ../flib/varint.fs
include ../flib/bme280.fs
include ../flib/tsl4531.fs
include ../mlib/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
