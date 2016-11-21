\ explore the bit-banged I2C driver
\ needs core.fs
cr cr reset

include ../flib/any/i2c-bb.fs
\ include ../flib/i2c/veml6040.fs

\ assumes that the VEML6040 sensor is connected to PB6..PB7

: go
  veml-init if ." can't find VEML6040" exit then
  begin
    500 ms
    cr
    micros veml-data 2>r 2>r micros swap - . ." µs: "
    ." r: " r> .  ." g: " r> .  ." b: " r> .  ." w: " r> .
  key? until ;

\ this causes folie to timeout on include matching, yet still starts running
\ 1234 ms go

+i2c i2c.
