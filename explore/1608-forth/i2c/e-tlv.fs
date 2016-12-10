\ TLV493 readout over I2C experiment
\ needs core.fs
cr cr reset
cr

\ include ../flib/stm32l0/i2c.fs
\ include ../flib/i2c/tlv493.fs

\ assumes that the TLV493 sensor is connected to PB6..PB7

: go
  tlv-init
  begin
    500 ms
    cr
    micros tlv-data >r >r >r micros swap - . ." µs: " r> r> r>
    ." x: " rot . ." y: " swap . ." z: " .
  key? until ;

i2c-init i2c? i2c.

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
