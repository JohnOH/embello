# SPI communication driver

[code]: any/spi-bb.fs (io)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/spi-bb.fs">any/spi-bb.fs</a>
* Needs: io

This describes the portable _bit-banged_ version of the SPI driver.

Each SPI transaction consists of the following steps:

* start the transaction by calling `+spi`
* call `>spi` to send a byte without reading the result
* call `spi>` to send a zero byte and return the result
* call `>spi>` to both send a byte and read back the result
* call any of the above as often as needed
* terminate the transaction by calling `-spi`

Before using SPI, you need to call `spi-init` to initialise the pins and
hardware device.

### API

[defs]: <> (spi-init +spi -spi)
```
: spi-init ( -- )  \ set up bit-banged SPI
: +spi ( -- ) ssel @ ioc! ;  \ select SPI
: -spi ( -- ) ssel @ ios! ;  \ deselect SPI
```

[defs]: <> (>spi spi> >spi>)
```
: >spi ( c -- ) >spi> drop ;  \ write byte to SPI
: spi> ( -- c ) 0 >spi> ;  \ read byte from SPI
: >spi> ( c -- c )  \ bit-banged SPI, 8 bits
```

### Variables

[defs]: <> (ssel)
```
PA4 variable ssel  \ pin used as slave select
```

### Constants

The `SCLK`, `MISO`, and `MOSI` constants should be defined _before_ including
this driver, if you want to use SPI on other pins than the default `PA5`, `PA6`,
and `PA7`, respectively.

The `ssel` variable is used during each transaction. It defaults to
`PA4`, but can be changed between transactions to connect to different slaves.

_Note:_ only the `ssel` pin set when `spi-init` is called will be properly
set up as GPIO output. To connect to addiitonal slave devices, you'll
need to initialise the other pins yourself, e.g. to use `PB0` as
additional slave select:

    PB0 ios!  OMODE-PP PB0 io-mode!

Once configured, this will let you switch to that slave using "`PB0 ssel !`".
