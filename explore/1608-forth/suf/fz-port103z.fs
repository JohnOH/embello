\ USB console for WaveShare-Port103Z boards

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
  %1111 12 lshift $40010800 bic!  \ PA3: output, push-pull, 2 MHz
  %0010 12 lshift $40010800 bis!
  3 bit $4001080C bis!  \ set PA3 high
  100000 0 do loop
  3 bit $4001080C bic!  \ set PA3 low

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include ../g6u/board.fs
include ../g6u/core.fs

hexdump
