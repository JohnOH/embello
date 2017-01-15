\ board definitions

\ eraseflash
compiletoflash
( board start: ) here dup hex.

\ RCC $18 + constant RCC-APB2ENR
\ RCC $14 + constant RCC-AHBENR

: jtag-deinit ( -- )  \ disable JTAG on PB3 PB4 PA15
  25 bit AFIO-MAPR bis! ;
: swd-deinit ( -- )  \ disable JTAG as well as PA13 and PA14
  AFIO-MAPR @ %111 24 lshift bic 26 bit or AFIO-MAPR ! ;

: list ( -- )  \ list all words in dictionary, short form
  cr dictionarystart begin
      dup 6 + ctype space
        dictionarynext until drop ;

include ../flib/mecrisp/calltrace.fs
include ../flib/mecrisp/cond.fs
include ../flib/mecrisp/hexdump.fs
include ../flib/stm32f1/clock.fs
include ../flib/stm32f1/io.fs
include ../flib/pkg/pins48.fs
include ../flib/stm32f1/spi.fs
include ../flib/stm32f1/timer.fs
include ../flib/stm32f1/pwm.fs
include ../flib/stm32f1/adc.fs
include ../flib/stm32f1/rtc.fs

PC13 constant LED

: led-on LED ioc! ;
: led-off LED ios! ;

: hello ( -- ) flash-kb . ." KB <prc2> " hwid hex.
  $10000 compiletoflash here -  flashvar-here compiletoram here -
  ." ram/flash: " . . ." free " ;

: init ( -- )  \ board initialisation
  init  \ this is essential to start up USB comms!
  ['] ct-irq irq-fault !  \ show call trace in unhandled exceptions
  swd-deinit  \ disable JTAG and SWD
  OMODE-PP LED io-mode!  led-off
  1000 systick-hz
\ hello ." ok." cr
;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
compiletoram
