\ hardware SPI driver

PA4  variable ssel   \ can be changed at run time

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
  OMODE-PP ssel @ io-mode! -spi
     OMODE-AF-PP PA5 io-mode!
     OMODE-AF-PP PA6 io-mode!
     OMODE-AF-PP PA7 io-mode!
  12 bit RCC-APB2ENR bis!  \ set SPI1EN
  %0000001101000100 SPI1-CR1 !  \ clk/2, i.e. 8 MHz, master
  SPI1-SR @ drop  \ appears to be needed to avoid hang in some cases
  2 bit SPI1-CR2 bis!  \ SS output enable
;
