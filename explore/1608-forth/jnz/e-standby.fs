\ standby mode experiment
\ needs core.fs
cr cr reset
cr

\ include ../flib/sleep-stm32l0.fs

     RCC $2C + constant RCC-IOPENR
     $40015804 constant DBG-CR

: standby ( -- )
  only-msi 
  0 RCC-APB2ENR !       \ disable USART1 and SPI1
\ 9 bit PWR-CR bis!     \ set ULP
  1 bit PWR-CR bis!     \ set PDDS for standby mode
\ 0 bit PWR-CR bis!     \ set LPSDSR
\ 29 bit EXTI-IMR bic!  \ clear IM29
  29 bit EXTI-EMR bis!  \ set EM29
\ -1 EXTI-PR !          \ clear all pending
  2 bit SCR bis!        \ set SLEEPDEEP
  wfe                   \ enter standby mode
;

rf69-init rf-sleep
2.1MHz 1000 systick-hz
\ %1 RCC-IOPENR !  \ disable all GPIO clocks, except GPIOA

( clock-hz    ) clock-hz @ .
( PWR-CR      ) PWR-CR @ hex.
( PWR-CSR     ) PWR-CSR @ hex.
( RCC-CR      ) RCC-CR @ hex.
( RCC-CSR     ) RCC-CSR @ hex.
( RCC-CCIPR   ) RCC-CCIPR @ hex.
( RCC-AHBENR  ) RCC-AHBENR @ hex.
( RCC-APB1ENR ) RCC-APB1ENR @ hex.
( RCC-APB2ENR ) RCC-APB2ENR @ hex.
( EXTI-IMR    ) EXTI-IMR @ hex.
( EXTI-EMR    ) EXTI-EMR @ hex.
( EXTI-PR     ) EXTI-PR @ hex.
( DBG-CR      ) DBG-CR @ hex.

led-off standby led-on
