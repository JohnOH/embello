\ USB console for generic F103 boards
\ self-contained, does not use the h, l, or d include files

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
compiletoflash

: bit ( u -- u )  \ turn a bit position into a single-bit mask
  1 swap lshift  1-foldable ;

include hal-stm32f1.fs
include ../flib/ring.fs

\ board-specific way to briefly pull USB-DP down
: usb-pulse ( -- )  \ toggle PA12, first down, then up
  %1111 16 lshift $40010804 bic!  \ PA12: output, push-pull, 2 MHz
  %0010 16 lshift $40010804 bis!  \ ... this affects CRH iso CRL
  12 bit $4001080C bic!  \ set PA12 low
  1200 0 do loop         \ approx 100 us delay
  12 bit $4001080C bis!  \ set PA12 high
;

include usb.fs

: init ( -- )  \ switch to USB as console
  72MHz  \ this is required for USB use
  10000000 0 do loop  \ approx 1s delay
  key? 0= if usb-io then ;  \ safety escape hatch

here hex.
cornerstone eraseflash

compiletoram
include ../mlib/hexdump.fs
hexdump
