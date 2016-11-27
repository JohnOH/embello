\ USB console for HyTiny-STM103T boards

$5000 eraseflashfrom  \ this must be loaded on top of a *clean* Mecrisp image!
cr compiletoflash

: bit ( u -- u )  \ turn a bit position into a single-bit mask
  1 swap lshift  1-foldable ;

include hal-stm32f1.fs
include ../flib/any/ring.fs
include usb.fs

: init ( -- )
\ init
  72MHz  \ this is required for USB use
\ key? if key if exit then then  \ safety escape hatch
  \ board-specific way to enable USB
  %1111 $40010800 bic!  \ PA0: output, push-pull, 2 MHz
  %0010 $40010800 bis!
  0 bit $4001080C bic!  \ set PA0 low
  usb-io  \ switch to USB as console
;

here hex.
cornerstone eraseflash
