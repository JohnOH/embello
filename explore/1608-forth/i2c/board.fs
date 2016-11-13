\ board definitions
\ needs always.fs

eraseflash
compiletoflash
( board start: ) here dup hex.

3 constant io-ports  \ A..C

include ../mlib/cond.fs
include ../mlib/hexdump.fs
include ../flib/io-stm32l0.fs
include ../flib/hal-stm32l0.fs
include ../flib/adc-stm32l0.fs
include ../flib/timer-stm32l0.fs
include ../flib/pwm-stm32l0.fs
include ../flib/spi-stm32l0.fs
include ../flib/i2c-stm32l0.fs

\ PB6 constant SCL
\ PB7 constant SDA
\ include ../flib/i2c-bb.fs

\ debug LEDs, connected to rightmost I/O pins on main header
PA0  constant LED1
PA1  constant LED2
PA2  constant LED3
PA3  constant LED4
PA11 constant LED5
PA12 constant LED6

: debug-pwm
\ FIXME set alt function #2 on all PWM pins, should be moved inside pwm driver
  $00002222 LED1 io-base GPIO.AFRL + !

  \ various duty cycles at 2 Hz
  2 LED1 +pwm   500 LED1 pwm
  2 LED2 +pwm  3500 LED2 pwm
  2 LED3 +pwm  6500 LED3 pwm
  2 LED4 +pwm  9500 LED4 pwm
;

PA15 constant LED

: led-on LED ioc! ;
: led-off LED ios! ;

: init ( -- )  \ board initialisation
  $00 hex.empty !  \ empty flash shows up as $00 iso $FF on these chips
  OMODE-PP LED io-mode!
\ 16MHz ( set by Mecrisp on startup to get an accurate USART baud rate )
  2 RCC-CCIPR !  \ set USART1 clock to HSI16, independent of sysclk
  flash-kb . ." KB <i2c> " hwid hex. ." ok." cr
  1000 systick-hz
;

: rx-connected? ( -- f )  \ true if RX is connected (and idle)
  IMODE-LOW PA10 io-mode!  PA10 io@ 0<>  OMODE-AF-PP PA10 io-mode!
  dup if 1 ms serial-key? if serial-key drop then then \ flush any input noise
;

: fake-key? ( -- f )  \ check for RX pin being pulled high
  rx-connected? if reset then false ;

\ unattended quits to the interpreter if the RX pin is connected, not floating
\ else it replaces the key? hook with a test to keep checking for RX reconnect
\ if so, it will reset to end up in the interpreter on the next startup
\ for use with a turnkey app in flash, i.e. ": init init unattended ... ;"

: unattended
  rx-connected? if quit then \ return to command prompt
  ['] fake-key? hook-key? ! ;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
