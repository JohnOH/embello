\ core libraries
\ needs board.fs
\ includes dev.fs

cr <<<hal-jnl>>>
cr
compiletoflash

( code start: ) here dup hex.

include ../flib/rf69.fs
include ../flib/varint.fs
include ../flib/bme280.fs
include ../flib/tsl4531.fs
include ../mlib/multi.fs

( flash use, code size: ) here dup hex. swap - .
cornerstone <<<lib-jnl>>>

include dev.fs
