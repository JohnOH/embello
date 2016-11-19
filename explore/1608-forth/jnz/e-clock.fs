\ clock experiments
\ needs core.fs
cr cr reset
cr

   USART1 $0 + constant USART1-CR1

: slow-usart1a
  1 RCC-CCIPR !  \ put USART1 on system clock
  0 bit USART1-CR1 bic!
  15 bit USART1-CR1 bis!  \ 8x iso 16x oversampling
\ 115200 2/ baud USART1-BRR !
  10 us
  34 USART1-BRR !
  10 us
  0 bit USART1-CR1 bis!
;

: slow-usart1b
\ 0 RCC-CCIPR !  \ put USART1 on system clock
  0 bit USART1-CR1 bic!
\ 115200 baud USART1-BRR !
\ 15000 baud USART1-BRR !
\ 18 USART1-BRR !
\ 19200 baud USART1-BRR !
  138 baud USART1-BRR !
  0 bit USART1-CR1 bis!
;

\ reduce systick at 65 KHz, else interrupts will eat up all the clock cycles
\ this means micros/us/millis/ms will all work, but 100x slower than usual
: slow 65KHz 10 systick-hz ;      \ 280 µA
: fast 2.1MHz 1000 systick-hz ;   \ 680 µA

: wait-for-key begin sleep ( led iox! ) key? until ;  \ 435 µA

: down   slow          wait-for-key           fast ;  \ 280 µA
: lost   slow only-msi wait-for-key           fast ;  \ 32 µA
: snooze slow 9 0 do i . 10 ( *100 ) ms loop  fast ;  \ 280 µA
: doze   slow only-msi   50 ( *100 ) ms       fast ;  \ 40 µA

: do-adc slow +adc adc-vcc . adc-temp . -adc  fast ;

: do-bme bme-init bme-calib slow bme-data fast bme-calc . . . ;
\ : do-tsl tsl-init slow tsl-data fast . ;

led-off
[IFDEF] rf69-init  rf69-init rf-sleep  [THEN]

( PWR-CR ) PWR-CR @ hex.
( CR ) RCC-CR @ hex.
( CFGR ) RCC-CFGR @ hex.
( ICSCR ) RCC-ICSCR @ hex.
( CCIPR ) RCC-CCIPR @ hex.
( BRR*100 ) 1152 baud .
( BRR*100 ) 192 baud .

( 16 MHz )  10 ms 16Mhz 16 .
( 2.1 MHz ) 10 ms 2.1Mhz 3 ms 123 .

\ ( BRR*100 ) 1152 baud .
\ ( CFGR ) RCC-CFGR @ hex.
\ ( 500 ms ) 1000 systick-hz 1 . 500 ms 2 .
\ 3 ms slow-usart1b 3 ms 123 .
