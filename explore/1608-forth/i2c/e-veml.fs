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
  if 10 bit I2C1-CR2 bis! then  \ RD_WRN
  13 bit I2C1-CR2 bis!  \ START
  begin 13 bit I2C1-CR2 bit@ 0= until  \ wait !START
;

: i2c-setn ( u -- )  \ prepare for N-byte transfer and reset buffer pointer
  16 lshift $FF00FFFF I2C1-CR2 bit@ or I2C1-CR2 !  i2c-reset ;
  
: n>i2c ( u -- )  \ send N bytes to the I2C interface
  i2c-setn
  begin
    begin 1 bit I2C1-ISR bit@ until  \ wait TXIS
    i2c> I2C1-TXDR c!
  again ;

: i2c>n ( u -- )  \ receive N bytes from the I2C interface
  i2c-setn
  begin
    begin 2 bit I2C1-ISR bit@ until  \ wait RXNE
    I2C1-RXDR c@ >i2c
  again ;

\ there are 4 cases:
\   tx>0 rx>0 : START - tx - RESTART - rx - STOP
\   tx>0 rx=0 : START - tx - STOP
\   tx=0 rx>0 : START - rx - STOP
\   tx=0 rx=0 : START - STOP          (used for presence detection)

: i2c-xfer ( u -- nak )
  i2c.ptr @ i2c.buf - ?dup if
    0 i2c-start n>i2c
    ?dup if  \ tx>0 rx>0
      1 i2c-start i2c>n
    else  \ tx>0 rx=0
    then
    i2c-stop
  else
    ?dup if  \ tx=0 rx>0
      1 i2c-start i2c>n
    else  \ tx=0 rx=0
    then
    i2c-stop
  then
  i2c-reset ack-nak ;

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
1234 ms go
