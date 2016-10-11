\ Try reading out the BME280 sensor.

\ define some missing constants
4 constant io-ports  \ A..D
RCC $18 + constant RCC-APB2ENR

include ../mlib/hexdump.fs
include ../flib/io-stm32f1.fs
include ../flib/pins64.fs
include ../flib/spi-stm32f1.fs

: ms 0 do 12000 0 do loop loop ;  \ assumes 72 MHz clock

PB6 constant SCL
PB7 constant SDA

include ../flib/i2c-bb.fs

PA5 constant LED1
PA1 constant LED2

: init-board ( -- )
  OMODE-PP LED1 io-mode!
  OMODE-PP LED2 io-mode!
;

: bme-init ( -- )
  +i2c
  $76 i2c-tx drop
  $F2 >i2c drop %1 >i2c drop
  $F4 >i2c drop %100111 >i2c drop
  $F5 >i2c drop %10100000 >i2c drop
  i2c-stop ;

32 buffer: bme.params  \ calibration data
 8 buffer: bme.values  \ last reading

: bme-rd ( reg -- )
  $76 i2c-tx drop
  >i2c drop i2c-stop
  $76 i2c-rx drop ;

: bme-1b ( addr nak -- addr+1 ) i2c> over c! 1+ ;

: bme-calib ( -- )
  bme.params
  $88 bme-rd  23 0 do 0 bme-1b loop  1 bme-1b  i2c-stop
  $A1 bme-rd                         1 bme-1b  i2c-stop
  $E1 bme-rd   6 0 do 0 bme-1b loop  1 bme-1b  i2c-stop
  drop ;

: bme-data ( -- )
  bme.values
  $F7 bme-rd  7 0 do 0 bme-1b loop  1 bme-1b  i2c-stop
  drop ;

: go
  init-board  bme-init  bme-calib
  bme.params 32 dump
  begin
    bme-data  bme.values 2@ cr hex. hex.
    1000 ms
  key? until ;
