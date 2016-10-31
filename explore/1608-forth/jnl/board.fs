\ board definitions
\ needs always.fs

cr eraseflash
compiletoflash

3 constant io-ports  \ A..C

include ../mlib/hexdump.fs
include ../flib/io-stm32l0.fs
include ../flib/hal-stm32l0.fs
include ../flib/adc-stm32l0.fs
include ../flib/timer-stm32l0.fs
include ../flib/pwm-stm32l0.fs
include ../flib/spi-stm32l0.fs
include ../flib/i2c-stm32l0.fs

\ PA4 variable ssel  \ can be changed at run time
\ PA5 constant SCLK
\ PA6 constant MISO
\ PA7 constant MOSI
\ include ../flib/spi-bb.fs

\ PB6 constant SCL
\ PB7 constant SDA
\ include ../flib/i2c-bb.fs

PA15 constant LED

: led-on led ioc! ;
: led-off led ios! ;

: init ( -- )  \ board initialisation
  $00 hex.empty !  \ empty flash shows up as $00 iso $FF on these chips
  OMODE-PP LED io-mode!
\ 16MHz ( set by Mecrisp on startup to get an accurate USART baud rate )
  2 RCC-CCIPR !  \ set USART1 clock to HSI16, independent of sysclk
  flash-kb . ." KB <jnl> " hwid hex. ." ok." cr
  1000 systick-hz
;

( flash use: ) here hex.
cornerstone <<<hal-jnl>>>
