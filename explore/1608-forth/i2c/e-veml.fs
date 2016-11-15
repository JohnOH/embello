\ VEML6040 readout over I2C experiment
\ needs core.fs
cr cr reset

include ../flib/i2c-stm32l0.fs
\ include ../flib/veml6040.fs

\ assumes that the VEML6040 sensor is connected to PB6..PB7

: veml-init ( -- )
  +i2c  $10 i2c-addr  3 0 do $00 >i2c loop  0 i2c-xfer drop ;

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

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
