\ core libraries
\ needs board.fs
\ includes dev.fs

cr <<<hal-jnl>>>
compiletoflash

here dup hex. ( code start )

include ../flib/rf69.fs
include ../flib/bme280.fs
include ../mlib/multi.fs

here dup hex. swap - . ( flash use, code size )
cornerstone <<<lib-jnl>>>

include dev.fs
