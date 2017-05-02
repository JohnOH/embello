\ USB console for HotMCU Core Board 1xx

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
cr
compiletoflash

include hal-stm32f1.fs
include ../flib/any/ring.fs
include usb.fs

: init ( -- )
  $FD RCC-APB2ENR !  \ enable AFIO and GPIOA..F clocks
  72MHz  \ this is required for USB use

  \ board-specific way to enable USB
  %1111 8 lshift $40011C04 bic!  \ PF10: output, push-pull, 2 MHz
  %0010 8 lshift $40011C04 bis!  \ ... this affects CRH iso CRL
  10 bit $40011C0C bis!  \ set PF10 high
  100000 0 do loop
  10 bit $40011C0C bic!  \ set PF10 low

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include ../g6u/board.fs
include ../g6u/core.fs

hexdump
