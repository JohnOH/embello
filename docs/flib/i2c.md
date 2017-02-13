# I2C communication driver

[code]: any/i2c-bb.fs (io)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/i2c-bb.fs">any/i2c-bb.fs</a>
* Needs: io

This describes the portable _bit-banged_ version of the I2C driver.

Each I2C transaction consists of the following steps:

* start the transaction by calling `i2c-addr`
* send all the bytes out with repeated calls to `>i2c` (or none at all)
* give the number of expected bytes read back to `i2c-xfer` (can be 0)
* check the result to verify that the device responded (false means ok)
* read the reply bytes with repeated calls to `i2c>` (or none at all)
* the transaction will be closed by the driver when the count is reaced

### API

[defs]: <> (i2c-init i2c-addr i2c-xfer >i2c i2c> i2c.)
```
: i2c-init ( -- )  \ initialise bit-banged I2C
: i2c-addr ( u -- )  \ start a new I2C transaction
: i2c-xfer ( u -- nak )  \ prepare for the reply
: >i2c ( u -- )  \ send one byte out to the I2C bus
: i2c> ( -- u )  \ read one byte back from the I2C bus
: i2c. ( -- )  \ scan and report all I2C devices on the bus
```

### Constants

The `SCL` and `SDA` constants should be defined _before_ including this driver,
if you want to use I2C on other pins than the default `PB6` and `PB7`,
respectively.
