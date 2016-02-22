\ bit-banged SPI driver

: +spi ( -- ) ssel @ io-0! ;  \ select SPI
: -spi ( -- ) ssel @ io-1! ;  \ deselect SPI

: >spi> ( c -- c )  \ bit-banged SPI, 8 bits
  8 0 do
    dup $80 and MOSI io!
    SCLK io-1!
    shl
    MISO io@ or
    SCLK io-0!
  loop
  $FF and ;

\ single byte transfers
: spi> ( -- c ) 0 >spi> ;  \ read byte from SPI
: >spi ( c -- ) >spi> drop ;  \ write byte to SPI

: spi-init ( -- )  \ set up bit-banged SPI
  OMODE-PP   ssel @ io-mode!
  OMODE-PP   SCLK io-mode!
  IMODE-OPEN MISO io-mode!
  OMODE-PP   MOSI io-mode!
  -spi spi> drop  \ cycle SCLK a few times with ssel off (high)
;
