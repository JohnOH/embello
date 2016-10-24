\ bit-banged SPI driver

: +spi ( -- ) ssel @ ioc! ;  \ select SPI
: -spi ( -- ) ssel @ ios! ;  \ deselect SPI

: >spi> ( c -- c )  \ bit-banged SPI, 8 bits
  8 0 do
    dup $80 and MOSI io!
    SCLK ios!
    shl
    MISO io@ or
    SCLK ioc!
  loop
  $FF and ;

\ single byte transfers
: spi> ( -- c ) 0 >spi> ;  \ read byte from SPI
: >spi ( c -- ) >spi> drop ;  \ write byte to SPI

: spi-init ( -- )  \ set up bit-banged SPI
  OMODE-PP  ssel @ io-mode! -spi
  OMODE-PP    SCLK io-mode!
  IMODE-FLOAT MISO io-mode!
  OMODE-PP    MOSI io-mode! ;
