\ USB console for Leaflabs Maple Mini and other similar boards

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
compiletoflash

include hal-stm32f1.fs
include ../flib/ring.fs
include usb.fs

: init ( -- )
  1000000 0 do loop  \ approx 1s delay
  72MHz  \ this is required for USB use
  key? if key if exit then then  \ safety escape hatch
  \ board-specific way to enable USB
  %1111 4 lshift $40010C04 bic!  \ PB9: output, push-pull, 2 MHz
  %0010 4 lshift $40010C04 bis!  \ ... this affects CRH iso CRL
  9 bit $40010C0C bis!  \ set PB9 high
  12000 0 do loop   \ approx 1 ms delay
  9 bit $40010C0C bic!  \ set PB9 low
  usb-io  \ switch to USB as console
;

here hex.
cornerstone eraseflash
