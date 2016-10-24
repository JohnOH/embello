\ Try out the STM32F103ZE "Basic" board.

\ 2 buttons, 2 LEDs, µSD, pads for I2C EEPROM and SPI flash, coin cell
\ two 2x32-pin headers for 112 I/O pins, shared with 2x17-pin LCD header

\ define some missing constants
7 constant io-ports  \ A..G
RCC $18 + constant RCC-APB2ENR

include ../mlib/hexdump.fs
include ../flib/io-stm32f1.fs
include ../flib/pins144.fs
include ../flib/spi-stm32f1.fs

PB6 constant SCL
PB7 constant SDA

include ../flib/i2c-bb.fs

PC0 constant LED1
PD3 constant LED2

: init-leds
  OMODE-PP LED1 io-mode!  LED1 ios!  \ inverted logic
  OMODE-PP LED2 io-mode!  LED2 ios!  \ inverted logic
;
