\ core libraries
\ needs board.fs
\ includes dev.fs

cr <<<hal-jnl>>>
compiletoflash

( code start: ) here dup hex.

include ../flib/rf69.fs
include ../flib/bme280.fs
include ../mlib/multi.fs

( flash use, code size: ) here dup hex. swap - .
cornerstone <<<lib-jnl>>>

include dev.fs
