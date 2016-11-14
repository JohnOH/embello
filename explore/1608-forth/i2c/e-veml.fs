\ VEML6040 readout over I2C experiment
\ needs core.fs
cr cr reset

include ../flib/i2c-stm32l0.fs
\ include ../flib/veml6040.fs

\ assumes that the VEML6040 sensor is connected to PB6..PB7

: veml-init ( -- )
  $10 i2c-tx drop  3 0 do  $00 >i2c drop  loop  i2c-stop ;

: veml-rd ( reg -- val )
  $10 i2c-tx drop >i2c drop i2c-stop
  $10 i2c-rx drop 0 i2c> 1 i2c> i2c-stop
  8 lshift or ;

: veml-data $08 veml-rd $09 veml-rd $0A veml-rd $0B veml-rd ;

: go
  veml-init
  begin
    500 ms
    cr
    micros veml-data 2>r 2>r micros swap - . ." µs: " 2r> 2r>
    ." w: " .  ." b: " .  ." g: " .  ." r: " .
  key? until ;

+i2c 100 ms i2c? \ i2c.

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
