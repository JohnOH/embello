\ USB console for Leaflabs Maple Mini and other similar boards

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
cr
compiletoflash

include hal-stm32f1.fs
include ../flib/any/ring.fs
include usb.fs

: init ( -- )
  $3D RCC-APB2ENR !  \ enable AFIO and GPIOA..D clocks
  72MHz  \ this is required for USB use

  \ board-specific way to enable USB
  %1111 4 lshift $40010C04 bic!  \ PB9: output, push-pull, 2 MHz
  %0010 4 lshift $40010C04 bis!  \ ... this affects CRH iso CRL
  9 bit $40010C0C bis!  \ set PB9 high
  100000 0 do loop
  9 bit $40010C0C bic!  \ set PB9 low

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include ../g6u/board.fs
include ../g6u/core.fs

hexdump
