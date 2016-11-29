\ board definitions

\ eraseflash
compiletoflash
( board start: ) here dup hex.

\ RCC $18 + constant RCC-APB2ENR
\ RCC $14 + constant RCC-AHBENR

: -jtag ( -- )  \ disable JTAG on PB3 PB4 PA15
  25 bit AFIO-MAPR bis! ;

: list ( -- )  \ list all words in dictionary, short form
  cr dictionarystart begin
      dup 6 + ctype space
        dictionarynext until drop ;

include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32f1/clock.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins64.fs
include ../flib/any/i2c-bb.fs
include ../flib/stm32f1/spi.fs
include ../flib/stm32f1/timer.fs
include ../flib/stm32f1/pwm.fs
include ../flib/stm32f1/adc.fs
include ../flib/stm32f1/rtc.fs

0 constant OLED.LARGE  \ display size: 0 = 128x32, 1 = 128x64 (default)

: hello ( -- ) flash-kb . ." KB <gu6> " hwid hex.
  $10000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ this is essential to start up USB comms!
  -jtag  \ disable JTAG, we only need SWD
  1000 systick-hz
\ hello ." ok." cr
;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
compiletoram
