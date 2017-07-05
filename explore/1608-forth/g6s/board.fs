\ board definitions

eraseflash
cr
compiletoflash

( board start: ) here dup hex.

4 constant io-ports  \ A..D

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins64.fs
include ../flib/stm32f1/hal.fs
include ../flib/stm32f1/spi.fs
include ../flib/stm32f1/i2c.fs
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
  jtag-deinit  \ disable JTAG, we only need SWD
  72MHz
  1000 systick-hz
  hello ." ok." cr
;

cornerstone <<<board>>>
hello
