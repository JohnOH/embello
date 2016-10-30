\ i2c experiment
\ needs core.fs
cr cr reset

include ../flib/i2c-stm32l0.fs
include ../flib/bme280.fs

\ assumes BME280 is present on PB6..PB7

$11000000 PB6 io-base GPIO.AFRL + !
    $00C0 PB6 io-base GPIO.OTYPER + h!

+i2c i2c? i2c.
1234 ms bme-init bme-calib bme-data bme-hpt cr tcalc . pcalc . hcalc .
