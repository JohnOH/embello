\ hardware SPI driver

[ifndef] ssel  PA4 variable ssel  [then]  \ can be changed at run time
[ifndef] SCLK  PA5 constant SCLK  [then]
[ifndef] MISO  PA6 constant MISO  [then]
[ifndef] MOSI  PA7 constant MOSI  [then]

$40013000 constant SPI1
     SPI1 $00 + constant SPI1-CR1
     SPI1 $04 + constant SPI1-CR2
     SPI1 $08 + constant SPI1-SR
     SPI1 $0C + constant SPI1-DR
\    SPI1 $10 + constant SPI1-CRCPR
\    SPI1 $14 + constant SPI1-RXCRCR
\    SPI1 $18 + constant SPI1-TXCRCR

: spi? ( -- )
  SPI1
  cr ."     CR1 " dup @ hex. 4 +
     ."     CR2 " dup @ hex. 4 +
     ."      SR " dup @ hex. 4 +
     ."      DR " dup @ hex. 4 +
  cr ."   CRCPR " dup @ hex. 4 +
     ."  RXCRCR " dup @ hex. 4 +
     ."  TXCRCR " dup @ hex. drop ;

: +spi ( -- ) ssel @ ioc! ;  \ select SPI
: -spi ( -- ) ssel @ ios! ;  \ deselect SPI

: >spi> ( c -- c )  \ hardware SPI, 8 bits
  SPI1-DR !  begin SPI1-SR @ 1 and until  SPI1-DR @ ;

\ single byte transfers
: spi> ( -- c ) 0 >spi> ;  \ read byte from SPI
: >spi ( c -- ) >spi> drop ;  \ write byte to SPI

: spi-init ( -- )  \ set up hardware SPI
  OMODE-PP    ssel @ io-mode! -spi
  OMODE-AF-PP SCLK   io-mode!
  OMODE-AF-PP MISO   io-mode!
  OMODE-AF-PP MOSI   io-mode!

  12 bit RCC-APB2ENR bis!  \ set SPI1EN
  %0000001101000100 SPI1-CR1 !  \ clk/2, i.e. 8 MHz, master
  SPI1-SR @ drop  \ appears to be needed to avoid hang in some cases
  2 bit SPI1-CR2 bis!  \ SS output enable
;
