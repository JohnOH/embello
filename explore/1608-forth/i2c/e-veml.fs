\ VEML6040 readout over I2C experiment
\ needs core.fs
cr cr reset

\ include ../flib/i2c-stm32l0.fs
\ include ../flib/veml6040.fs

100 buffer: i2c.buf
 0 variable i2c.ptr
 0 variable i2c.hdr

: i2c-reset ( -- )  i2c.buf i2c.ptr ! ;

: i2c-addr ( u -- )  shl i2c.hdr c!  i2c-reset ;

: i2c++ ( -- addr )  i2c.ptr @  1 i2c.ptr +! ;

: >i2c ( u -- ) i2c++ c! ;
: i2c> ( -- u ) i2c++ c@ ;

: n>i2c ( u -- )
  drop
;

: i2c>n ( u -- )
  drop
;

: i2c-txrx ;
: i2c-tx ;
: i2c-rx ;

\ there are 4 cases:
\   tx>0 rx>0 : START - tx - RESTART - rx - STOP
\   tx>0 rx=0 : START - tx - STOP
\   tx=0 rx>0 : START - rx - STOP
\   tx=0 rx=0 : START - STOP          (used for presence detection)

: i2c-xfer ( u -- f )
  i2c.ptr @ i2c.buf - ?dup if
    n>i2c
    ?dup if
      i2c-txrx \ tx>0 rx>0
    else
      i2c-tx \ tx>0 rx=0
    then
  else
    ?dup if
      i2c-rx \ tx=0 rx>0
    else
      i2c-tx \ tx=0 rx=0
    then
  then
  i2c-reset ;

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
