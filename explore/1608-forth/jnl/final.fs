\ burn the final application in flash for self-contained use

include board.fs
include core.fs
include main.fs

: init init unattended main ;

( flash end, ram free: ) here hex. compiletoram flashvar-here here - .
