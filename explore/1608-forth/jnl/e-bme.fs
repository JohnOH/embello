\ BME280 readout over I2C experiment
\ needs core.fs
cr cr reset

\ include ../flib/i2c-stm32l0.fs
\ include ../flib/bme280.fs

\ assumes BME280 is present on PB6..PB7

: go
  bme-init bme-calib
  begin
    cr
    micros bme-data bme-calc >r >r >r micros swap - . ." µs: " r> r> r>
    . ." °C " . ." hPa " . ." %RH (all x100)"
    500 ms
    $30 i2c-tx drop i2c-stop  \ FIXME hangs with back-to-back accesses to $29!
  key? until ;

+i2c i2c? i2c.

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
