\ install the UART2 words, as well as DISK.DATA and DISK.SIZE

\ <<<core>>>
compiletoflash
include ../flib/stm32f1/uart2.fs
include cpm/disk.fs
compiletoram
hello

include dev.fs
