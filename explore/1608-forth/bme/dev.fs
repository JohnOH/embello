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

32 buffer: params  \ calibration data
 8 buffer: values  \ last reading
0 variable tfine   \ used for p & h compensation

: bme-rd ( reg -- )
  $76 i2c-tx drop
  >i2c drop i2c-stop
  $76 i2c-rx drop ;

: bme-i2c+ ( addr nak -- addr+1 ) i2c> over c! 1+ ;

: bme-calib ( -- )
  params
  $88 bme-rd  23 0 do 0 bme-i2c+ loop  1 bme-i2c+  i2c-stop
  $A1 bme-rd                           1 bme-i2c+  i2c-stop
  $E1 bme-rd   6 0 do 0 bme-i2c+ loop  1 bme-i2c+  i2c-stop
  drop ;

: bme-u16 ( off -- val ) params + dup c@ swap 1+ c@ 8 lshift or ;
: bme-s16 ( off -- val ) bme-u16 16 lshift 16 arshift ;
: bme-u20be ( off -- val )
  values + dup c@ 12 lshift swap 1+
           dup c@  4 lshift swap 1+
               c@  4 rshift  or or ;

: bme-data ( -- )
  values
  $F7 bme-rd  7 0 do 0 bme-i2c+ loop  1 bme-i2c+  i2c-stop
  drop ;

: bme-hpt ( -- rawh rawp rawt )
  values 6 + dup c@ 8 lshift swap 1+ c@ or  0 bme-u20be  3 bme-u20be ;

: tcalc ( rawt -- t100 )
  3 rshift dup shr swap
  ( T1: ) 0 bme-u16 shl - ( T2: ) 2 bme-s16 * 11 arshift
  swap ( T1: ) 0 bme-u16 - dup * 12 arshift ( T3: ) 4 bme-s16 * 14 arshift +
  dup tfine !  5 * 128 + 8 arshift ;

: go
  init-board  bme-init  bme-calib
  params 32 dump
  begin
    bme-data bme-hpt
    cr tcalc . hex. hex.
    1000 ms
  key? until ;
