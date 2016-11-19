\ core libraries

<<<board>>>
cr compiletoflash
( core start: ) here dup hex.

include ../flib/rf69.fs
include ../flib/varint.fs
include ../flib/mcp3424.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
