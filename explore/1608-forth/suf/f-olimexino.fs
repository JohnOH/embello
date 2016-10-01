\ USB console for Olimexino-STM32 and other Leaflabs Maple-like boards
\ self-contained, does not use the h, l, or d include files

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
compiletoflash

: bit ( u -- u )  \ turn a bit position into a single-bit mask
  1 swap lshift  1-foldable ;

include hal-stm32f1.fs
include ../flib/ring.fs

\ board-specific way to briefly pull USB-DP down
: usb-pulse ( -- )  \ toggle PC12, first up, then down (due to inverted logic)
  %1111 16 lshift $40011004 bic!  \ PC12: output, push-pull, 2 MHz
  %0010 16 lshift $40011004 bis!  \ ... this affects CRH iso CRL
  12 bit $4001100C bis!  \ set PC12 high
  12000 0 do loop        \ approx 1ms delay
  12 bit $4001100C bic!  \ set PC12 low
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
