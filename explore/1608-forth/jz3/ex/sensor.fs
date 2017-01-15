\ Read out the BME280 sensor, see http://jeelabs.org/article/1651d

forgetram

\ include ../flib/i2c/bme280.fs

\ use PA13 and PA14 to supply power to the BME280 sensor
: bme-power
  OMODE-PP PA14 io-mode!  PA14 ioc!  \ set PA14 to "0", acting as ground
  OMODE-PP PA13 io-mode!  PA13 ios!  \ set PA13 to "1", acting as +3.3V
;

\ configure I2C and the BME280 sensor attached to it
: setup  bme-power bme-init . bme-calib ;

\ print BME readings every 500 ms, until new input is received from Folie
: go
  begin
    bme-data bme-calc
    cr . . .
    500 ms
  key? until ;

\ the delay is a hack to force a timeout in Folie before the loop starts
1234 ms setup go
