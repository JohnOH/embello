\ install the UART2 words, as well as DISK.DATA and DISK.SIZE

\ <<<core>>>
compiletoflash
include ../flib/stm32f1/uart2.fs
compiletoram
hello

include dev.fs
