\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

include ../flib/rf69.fs
include ../flib/varint.fs
include ../flib/bme280.fs
include ../flib/tsl4531.fs
include ../flib/veml6040.fs
include ../flib/mag3110.fs
include ../flib/oled.fs
include ../mlib/graphics.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
