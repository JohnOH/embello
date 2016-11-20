\ BME280 readout over I2C experiment
\ needs core.fs
cr cr reset

include ../flib/stm32l0/i2c.fs
include ../flib/i2c/bme280.fs

\ assumes that the BME280 sensor is connected to PB6..PB7

: .2 ( n -- )  \ display value with two decimal points
  0 swap 0,01 f* 0,005 d+ 2 f.n ;

: go
  bme-init if ." can't find BME280" exit then bme-calib
  begin
    500 ms
    cr
    micros bme-data bme-calc >r >r >r micros swap - . ." µs: " r> r> r>
    .2 ." °C " .2 ." hPa " .2 ." %RH "
  key? until ;

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
