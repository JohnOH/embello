\ USB console for generic F103 boards

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
cr
cr compiletoflash

: bit ( u -- u )  \ turn a bit position into a single-bit mask
  1 swap lshift  1-foldable ;

include hal-stm32f1.fs
include ../flib/any/ring.fs
include usb.fs

: init ( -- )
  72MHz  \ this is required for USB use
  \ board-specific way to enable USB
  %1111 16 lshift $40010804 bic!  \ PA12: output, push-pull, 2 MHz
  %0010 16 lshift $40010804 bis!  \ ... this affects CRH iso CRL
  12 bit $4001080C bic!  \ set PA12 low
  100000 0 do loop
  12 bit $4001080C bis!  \ set PA12 high
  usb-io  \ switch to USB as console
;

here hex.
cornerstone eraseflash
