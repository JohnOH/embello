\ Read out the tmp102 temperature sensor on i2c-bus
\ see http://jeelabs.net/boards/7/topics/7417

[ifndef] TMP.ADDR  $48 constant TMP.ADDR  [then]

\ use PA13 and PA14 to supply power to the TMP102 sensor
: tmp102-power
  OMODE-PP PA14 io-mode!  PA14 ioc!  \ set PA14 to "0", acting as ground
  OMODE-PP PA13 io-mode!  PA13 ios!  \ set PA13 to "1", acting as +3.3V
;

: tmp102-init ( -- )  \ initialise the TMP102, assuming it's powered by IO pins
  tmp102-power  50 ms  i2c-init ;

: tmp102  ( -- i )                           \ returns temp * 10 on the stack
  TMP.ADDR i2c-addr  0 >i2c  2 i2c-xfer drop  i2c>h   \ sensor read out
  dup $FF and swap 8 rshift swap 8 lshift + 4 rshift  \ calculate temp
  dup $800 and if $7FF and negate then
  625 * 1000 / ;
