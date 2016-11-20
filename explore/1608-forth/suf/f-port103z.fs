\ USB console for WaveShare-Port103Z boards

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
compiletoflash

include hal-stm32f1.fs
include ../flib/any/ring.fs
include usb.fs

: init ( -- )
  1000000 0 do loop  \ approx 1s delay
  72MHz  \ this is required for USB use
  key? if key if exit then then  \ safety escape hatch
  \ board-specific way to enable USB
  %1111 12 lshift $40010800 bic!  \ PA3: output, push-pull, 2 MHz
  %0010 12 lshift $40010800 bis!
  3 bit $4001080C bic!  \ set PA3 low
  usb-io  \ switch to USB as console
;

here hex.
cornerstone eraseflash
