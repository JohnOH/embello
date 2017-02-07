\ burn the final application in flash for self-contained use

include ../jz3/always.fs
include board.fs
include core.fs

compiletoflash
( main start: ) here dup hex.

include main.fs

: init init unattended main ;
( main end, ram free: ) here hex. compiletoram flashvar-here here - .
