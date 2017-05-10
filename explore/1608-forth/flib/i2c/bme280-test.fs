\ tests for bme280.fs
\ the tests assume an attached BME280 in a comfy room environment

include bme280.fs
include ../any/testing.fs

: stack-empty? depth 0 =always ;

bme-reset stack-empty?
bme-init 0 =always
bme-calib
stack-empty?

100 ms
bme-data bme-calc .v ( h p t )
dup 1000 > always \ temp should be >10C
    4000 < always \ temp should be <40C
dup  90000 > always \ pressure should be >900kPa
    120000 < always \ pressure should be <1200kPa
dup 2000 > always \ humidity should be >20%
    8000 < always \ humidity should be <80%

test-summary
