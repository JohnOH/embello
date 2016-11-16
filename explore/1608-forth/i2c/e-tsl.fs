\ TSL4531 readout over I2C experiment
\ needs core.fs
cr cr reset

\ include ../flib/i2c-stm32l0.fs
include ../flib/tsl4531.fs

\ assumes that the TSL4531 sensor is connected to PB6..PB7

: go
  tsl-init
  begin
    500 ms
    cr
    micros tsl-data micros rot - . ." µs: "
    . ." lux "
  key? until ;

+i2c i2c? \ i2c.

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
