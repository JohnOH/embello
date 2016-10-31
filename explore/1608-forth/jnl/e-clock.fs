\ clock experiments
\ needs core.fs
cr cr reset
cr
     RCC $04 + constant RCC-ICSCR
     RCC $4C + constant RCC-CCIPR
   USART1 $0 + constant USART1-CR1

$40007000 constant PWR-CR

: 2.1MHz ( -- )  \ set the main clock to 2.1 MHz, keep baud rate at 115200
  %010 13 lshift RCC-ICSCR bic!
  %101 13 lshift RCC-ICSCR bis!
  8 bit RCC-CR bis!               \ set MSION
  begin 9 bit RCC-CR bit@ until   \ wait for MSIRDY
  %00 RCC-CFGR !                  \ revert to MSI @ 2.1 MHz, no PLL
  $101 RCC-CR !                   \ turn off HSE, and PLL
  2097000 clock-hz !  \ 115200 baud USART1-BRR !  \ fix console baud rate
;

: 16MHz ( -- )  \ set the main clock to 16 MHz, keep baud rate at 115200
  0 bit RCC-CR bis!               \ set HSI16ON
  begin 2 bit RCC-CR bit@ until   \ wait for HSI16RDYF
  %01 RCC-CFGR !                  \ revert to HSI16, no PLL
  0 bit RCC-CR !                  \ turn off MSI, HSE, and PLL
2 2 RCC-CCIPR !                   \ switch USART1 clock to HSI16
  16000000 clock-hz !  115200 baud USART1-BRR !  \ fix console baud rate
;

( PWR-CR ) PWR-CR @ hex.
( CR ) RCC-CR @ hex.
( CFGR ) RCC-CFGR @ hex.
( ICSCR ) RCC-ICSCR @ hex.
( CCIPR ) RCC-CCIPR @ hex.
( BRR*100 ) 1152 baud .
( BRR*100 ) 192 baud .

( 16 MHz )  10 ms 16Mhz 16 .
( 2.1 MHz ) 10 ms 2.1Mhz 3 ms 123 .

: slow-usart1a
  1 RCC-CCIPR !  \ put USART1 on system clock
  0 bit usart1-cr1 bic!
  15 bit usart1-cr1 bis!  \ 8x iso 16x oversampling
\ 115200 2/ baud USART1-BRR !
  10 us
  34 USART1-BRR !
  10 us
  0 bit usart1-cr1 bis!
;

: slow-usart1b
\ 0 RCC-CCIPR !  \ put USART1 on system clock
  0 bit usart1-cr1 bic!
\ 115200 baud USART1-BRR !
\ 15000 baud USART1-BRR !
\ 18 USART1-BRR !
\ 19200 baud USART1-BRR !
  138 baud USART1-BRR !
  0 bit usart1-cr1 bis!
;

\ ( BRR*100 ) 1152 baud .
\ ( CFGR ) RCC-CFGR @ hex.
\ ( 500 ms ) 1000 systick-hz 1 . 500 ms 2 .
\ 3 ms slow-usart1b 3 ms 123 .

: 65KHz ( -- )
  %111 13 lshift RCC-ICSCR bic!
  65000 clock-hz ! ;

: hsi-off $100 RCC-CR ! ;
: hsi-on $101 RCC-CR ! ;

: led-off led ios! ;
: led-on led ioc! ;

: wait-for-key begin sleep ( led iox! ) key? until ;
: reduce rf69-init rf-sleep led-off 2.1MHz ;

: slow reduce 65KHz 1000 systick-hz ;
: fast       2.1mhz 1000 systick-hz ;

\ FIXME various issues
: do-adc slow +adc adc-vcc . adc-temp . -adc  fast ;
: do-bme +i2c bme-init bme-calib slow bme-data fast bme-calc . . . ;
: do-tsl +i2c tsl-init           slow tsl-data fast .              ;

: down   slow         wait-for-key            fast ;  \ 680 µA
: lost   slow hsi-off wait-for-key            fast ;  \ 280 µA
: snooze slow 6 1 do i . 10000 0 do loop loop fast ;  \ 280 µA
: doze   slow hsi-off    50000 0 do loop      fast ;  \ 50 µA

