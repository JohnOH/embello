\ hardware SPI driver

$40013000 constant SPI1
     SPI1 $0 + constant SPI1-CR1
     SPI1 $4 + constant SPI1-CR2
     SPI1 $8 + constant SPI1-SR
     SPI1 $C + constant SPI1-DR

: spi. ( -- )  \ display SPI hardware registers
  cr ." CR1 " SPI1-CR1 @ h.4
    ."  CR2 " SPI1-CR2 @ h.4
     ."  SR " SPI1-SR @ h.4 ;

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
  IMODE-FLOAT PA6 io-mode!
  OMODE-AF-PP PA7 io-mode!
  12 bit RCC-APB2ENR bis!  \ set SPI1EN
  %0000000001010100 SPI1-CR1 !  \ clk/8, i.e. 9 MHz, master
  SPI1-SR @ drop  \ appears to be needed to avoid hang in some cases
  2 bit SPI1-CR2 bis!  \ SS output enable
;

$40003800 constant SPI2
     SPI2 $0 + constant SPI2-CR1
     SPI2 $4 + constant SPI2-CR2
     SPI2 $8 + constant SPI2-SR
     SPI2 $C + constant SPI2-DR

: spi2. ( -- )  \ display SPI hardware registers
  cr ." CR1 " SPI2-CR1 @ h.4
    ."  CR2 " SPI2-CR2 @ h.4
     ."  SR " SPI2-SR @ h.4 ;

: +spi2 ( -- ) ssel2 @ ioc! ;  \ select SPI
: -spi2 ( -- ) ssel2 @ ios! ;  \ deselect SPI

: >spi2> ( c -- c )  \ hardware SPI, 8 bits
  SPI2-DR !  begin SPI2-SR @ 1 and until  SPI2-DR @ ;

\ single byte transfers
: spi2> ( -- c ) 0 >spi2> ;  \ read byte from SPI
: >spi2 ( c -- ) >spi2> drop ;  \ write byte to SPI

: spi2-init ( -- )  \ set up hardware SPI
  OMODE-PP ssel2 @ io-mode! -spi2
  OMODE-AF-PP PB13 io-mode!
  IMODE-FLOAT PB14 io-mode!
  OMODE-AF-PP PB15 io-mode!
  14 bit RCC-APB1ENR bis!  \ set SPI2EN
  %0000000001001100 SPI2-CR1 !  \ clk/4, i.e. 9 MHz, master
  SPI2-SR @ drop  \ appears to be needed to avoid hang in some cases
  2 bit SPI2-CR2 bis!  \ SS output enable
;
