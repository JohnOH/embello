\ USB console for Olimexino-STM32 and other Leaflabs Maple-like boards

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
  %1111 16 lshift $40011004 bic!  \ PC12: output, push-pull, 2 MHz
  %0010 16 lshift $40011004 bis!  \ ... this affects CRH iso CRL
  12 bit $4001100C bis!  \ set PC12 high
  100000 0 do loop
  12 bit $4001100C bic!  \ set PC12 low

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include ../g6u/board.fs
include ../g6u/core.fs

hexdump
