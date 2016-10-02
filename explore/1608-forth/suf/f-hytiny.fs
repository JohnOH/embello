\ USB console for HyTiny-STM103T boards
\ self-contained, does not use the h, l, or d include files

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
compiletoflash

: bit ( u -- u )  \ turn a bit position into a single-bit mask
  1 swap lshift  1-foldable ;

include hal-stm32f1.fs
include ../flib/ring.fs

\ board-specific way to briefly pull USB-DP down
: usb-pulse ( -- )  \ toggle PA0, first up, then down (due to inverted logic)
  %1111 $40010800 bic! %0010 $40010800 bis!  \ PA0: output, push-pull, 2 MHz
  1 $4001080C bis!  \ set PA0 high
  1200 0 do loop    \ approx 100 us delay
  1 $4001080C bic!  \ set PA0 low
;

include usb.fs

: init ( -- )  \ switch to USB as console
  1000000 0 do loop  \ approx 1s delay
  72MHz  \ this is required for USB use
  key? 0= if usb-io then ;  \ safety escape hatch

here hex.
cornerstone eraseflash

compiletoram
include ../mlib/hexdump.fs
hexdump
