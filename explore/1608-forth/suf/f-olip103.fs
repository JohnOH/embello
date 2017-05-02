\ USB console for Olimex P103 board

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
  %1111 12 lshift $40011004 bic!  \ PC11: output, push-pull, 2 MHz
  %0010 12 lshift $40011004 bis!  \ ... this affects CRH iso CRL
  11 bit $4001100C bis!  \ set PC11 high
  100000 0 do loop
  11 bit $4001100C bic!  \ set PC11 low

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include ../g6u/board.fs
include ../g6u/core.fs

hexdump
