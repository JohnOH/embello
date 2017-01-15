\ MAG3110 readout over I2C experiment
\ needs core.fs
cr cr reset

\ include ../flib/stm32l0/i2c.fs
include ../flib/i2c/mag3110.fs

\ assumes that the MAG3110 sensor is connected to PB6..PB7

: go
  mag-init if ." can't find MAG3110" exit then
  begin
    500 ms
    cr
    micros mag-data >r >r >r micros swap - . ." µs: "
    ." x: " r> .  ." y: " r> .  ." z: " r> .
  key? until ;

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
