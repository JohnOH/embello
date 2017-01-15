\ TSL4531 readout over I2C experiment
\ needs core.fs
cr cr reset

\ include ../flib/stm32l0/i2c.fs
include ../flib/i2c/tsl4531.fs

\ assumes that the TSL4531 sensor is connected to PB6..PB7

: go
  tsl-init if ." can't find TSL4531" exit then
  begin
    500 ms
    cr
    micros tsl-data micros rot - . ." µs: "
    . ." lux "
  key? until ;

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
