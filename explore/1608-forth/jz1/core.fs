\ core libraries

<<<board>>>
compiletoflash
( core start: ) here dup hex.

\ include ../flib/spi/rf69.fs
\ include ../flib/any/varint.fs
\ include ../flib/i2c/bme280.fs
\ include ../flib/i2c/tsl4531.fs
\ include ../flib/i2c/veml6040.fs
\ include ../flib/i2c/mag3110.fs
\ include ../flib/i2c/ssd1306.fs
\ include ../flib/mecrisp/graphics.fs
include ../flib/mecrisp/multi.fs

( core end, size: ) here dup hex. swap - .
cornerstone <<<core>>>
compiletoram
