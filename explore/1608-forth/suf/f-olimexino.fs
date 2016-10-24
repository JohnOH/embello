\ USB console for Olimexino-STM32 and other Leaflabs Maple-like boards

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
  %1111 16 lshift $40011004 bic!  \ PC12: output, push-pull, 2 MHz
  %0010 16 lshift $40011004 bis!  \ ... this affects CRH iso CRL
  12 bit $4001100C bic!  \ set PC12 low
  usb-io  \ switch to USB as console
;

here hex.
cornerstone eraseflash
