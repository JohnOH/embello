\ board definitions

\ eraseflash
compiletoflash
( board start: ) here dup hex.

4 constant io-ports  \ A..D

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins64.fs
include ../flib/stm32f1/hal.fs
include ../flib/any/i2c-bb.fs
include ../flib/stm32f1/spi.fs
include ../flib/stm32f1/timer.fs
include ../flib/stm32f1/pwm.fs
include ../flib/stm32f1/adc.fs
include ../flib/stm32f1/rtc.fs

: hello ( -- ) flash-kb . ." KB <g6s> " hwid hex.
  $10000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ uses new uart init convention
  ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
  -jtag  \ disable JTAG, we only need SWD
  1000 systick-hz
  hello ." ok." cr
;

: forgetram ( -- )  \ remove all definitions in RAM without requiring a reset
  compiletoram
  \ these values are build/version/arch-specific!!!
  $4F28 @ $4F30 @ !  \ RamDictionaryAnfang  Dictionarypointer !
  $4F38 @ $4F34 @ !  \ CoreDictionaryAnfang Fadenende !
;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
