\ Si570 control over I2C experiment

forgetram ."  ok." cr quit

\ assumes that the Si570 sensor is connected to PB6..PB7

include ../flib/i2c/si570.fs

: go
  si570-init .
  si.buf hex.
  ." rfreq: " si.mul @ .
\ si.buf 6 dump
  5 begin
    cr dup .
    dup si570-freq
\   si.buf 6 dump
    5 +
    3000 ms
  key? until drop ;

\ i2c-init i2c.

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
