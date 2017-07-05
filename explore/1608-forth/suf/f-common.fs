\ USB console for HyTiny-STM32F103TB and BluePill-STM32F103C8 boards

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
  flash-kb 128 = if
    \ it looks like it's a HyTiny ...
    %1111 $40010800 bic!  \ PA0: output, push-pull, 2 MHz
    %0010 $40010800 bis!
    0 bit $4001080C bis!  \ set PA0 high
    100000 0 do loop
    0 bit $4001080C bic!  \ set PA0 low
  else
    \ nope, it must be a Blue Pill or similar ...
    %1111 16 lshift $40010804 bic!  \ PA12: output, push-pull, 2 MHz
    %0010 16 lshift $40010804 bis!  \ ... this affects CRH iso CRL
    12 bit $4001080C bic!  \ set PA12 low
    100000 0 do loop
    12 bit $4001080C bis!  \ set PA12 high
  then

  usb-io  \ switch to USB as console
;

( usb end: ) here hex.
cornerstone eraseflash

include ../g6u/board.fs
include ../g6u/core.fs

hexdump
