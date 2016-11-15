\ VEML6040 readout over I2C experiment
\ needs core.fs
cr cr reset

\ include ../flib/i2c-stm32l0.fs
\ include ../flib/veml6040.fs

100 buffer: i2c.buf
 0 variable i2c.ptr

: i2c-reset ( -- )  i2c.buf i2c.ptr ! ;

: i2c-addr ( u -- )  shl I2C1-CR2 !  i2c-reset ;

: i2c++ ( -- addr )  i2c.ptr @  1 i2c.ptr +! ;

: >i2c ( u -- ) i2c++ c! ;
: i2c> ( -- u ) i2c++ c@ ;

: i2c-start ( rd -- )
  $3F38 I2C1-ICR !  \ clear all flags
  if 10 bit I2C1-CR2 bis! then  \ RD_WRN
  13 bit I2C1-CR2 bis!  \ START
\ begin 13 bit I2C1-CR2 bit@ not until  \ wait for !START
;

: i2c-setn ( u -- )  \ prepare for N-byte transfer and reset buffer pointer
  16 lshift I2C1-CR2 @ $FF00FFFF and or I2C1-CR2 !  i2c-reset ;
  
: i2c-wr ( -- )  \ send bytes to the I2C interface
\ ." N> " i2c? cr
  begin
    11 .
    begin %111001 I2C1-ISR bit@ until  \ wait for TC, STOPF, NACKF, or TXE
\   begin $1 I2C1-ISR bit@ until  \ wait TXE
    12 .
\ 1 bit I2C1-ISR bit@ while  \ while TXIS
  6 bit I2C1-ISR bit@ not while  \ while !TC
    i2c> 13 . dup . I2C1-TXDR !
  repeat 14 . cr ;

: i2c-rd ( -- )  \ receive bytes from the I2C interface
\ ." N< " i2c? cr
  begin
    21 .
    begin %111100 I2C1-ISR bit@ until  \ wait for TC, STOPF, NACKF, or RXNE
    22 .
  6 bit I2C1-ISR bit@ not while  \ while !TC
    I2C1-RXDR @ 23 . dup . >i2c
  repeat 24 . cr ;

\ there are 4 cases:
\   tx>0 rx>0 : START - tx - RESTART - rx - STOP
\   tx>0 rx=0 : START - tx - STOP
\   tx=0 rx>0 : START - rx - STOP
\   tx=0 rx=0 : START - STOP          (used for presence detection)

: i2c-xfer ( u -- nak )
  i2c.ptr @ i2c.buf - ?dup if
    i2c-setn  0 i2c-start  i2c-wr  \ tx>0
  else
    dup 0= if 0 i2c-start then  \ tx=0 rx=0
  then
  ?dup if
    i2c-setn  1 i2c-start  i2c-rd  \ rx>0
  then
  i2c-stop ack-nak i2c-reset ;

: i2c. ( -- )  \ scan and report all I2C devices on the bus
  128 0 do
    cr i h.2 ." :"
    16 0 do  space
      i j +
      dup $08 < over $77 > or if drop 2 spaces else
        dup i2c-addr  0 i2c-xfer  if drop ." --" else h.2 then
      then
    loop
  16 +loop ;

\ assumes that the VEML6040 sensor is connected to PB6..PB7

: veml-init ( -- )
  $10 i2c-addr  3 0 do $00 >i2c loop  0 i2c-xfer drop ;

: veml-rd ( reg -- val )
  $10 i2c-addr  >i2c  2 i2c-xfer drop
  i2c> i2c> 8 lshift or ;

: veml-data $8 veml-rd $9 veml-rd $A veml-rd $B veml-rd ;

: go
  veml-init
  begin
    500 ms
    cr
    micros veml-data 2>r 2>r micros swap - . ." µs: " 2r> 2r>
    ." w: " .  ." b: " .  ." g: " .  ." r: " .
  key? until ;

+i2c 100 ms i2c? \ i2c.
i2c.buf 100 0 fill

\ this causes folie to timeout on include matching, yet still starts running
\ 1234 ms go

cr veml-init 50 ms veml-data . . . .
