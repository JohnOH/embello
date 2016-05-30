\ USB console for LCTech boards
\ self-contained, does not use the h, l, or d include files

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
compiletoflash

4 constant io-ports  \ A..D

include ../mlib/hexdump.fs
include ../flib/io-stm32f1.fs
include ../flib/hal-stm32f1.fs
include ../flib/ring.fs

: init ( -- )  \ board initialisation
  -jtag  \ disable JTAG, we only need SWD
  72MHz
  flash-kb . ." KB <suf> " hwid hex. ." ok." cr
  1000 systick-hz ;

\ board-specific way to briefly pull USB-DP down
: usb-pulse OMODE-OD PA12 io-mode!  PA12 ioc!  1 ms  PA12 ios! ;

include usb.fs

: init ( -- ) init 2000 ms key? 0= if usb-io then ;  \ safety escape hatch

cornerstone eraseflash
hexdump
