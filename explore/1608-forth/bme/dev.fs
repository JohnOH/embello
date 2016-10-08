\ Try reading out the BME280 sensor.

\ define some missing constants
4 constant io-ports  \ A..D
RCC $18 + constant RCC-APB2ENR

include ../mlib/hexdump.fs
include ../flib/io-stm32f1.fs
include ../flib/pins64.fs
include ../flib/spi-stm32f1.fs

PB6 constant SCL
PB7 constant SDA

include ../flib/i2c-bb.fs

PA5 constant LED1
PA1 constant LED2

: init-board ( -- )
  OMODE-PP LED1 io-mode!
  OMODE-PP LED2 io-mode!
  +i2c
;

\ i2c.
