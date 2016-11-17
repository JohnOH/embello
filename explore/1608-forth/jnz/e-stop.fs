\ stop mode experiment
\ needs core.fs
cr cr reset
cr

include ../flib/sleep-stm32l0.fs

     RCC $2C + constant RCC-IOPENR
     $40015804 constant DBG-CR

: lp-blink ( -- )
  only-msi
  0 RCC-APB2ENR !  \ disable USART1 and SPI1
  begin
    stop1s stop1s stop1s
    led-on 10 ms led-off
  again ;

rf69-init rf-sleep
2.1MHz 1000 systick-hz
\ %1 RCC-IOPENR !  \ disable all GPIO clocks, except GPIOA

+lptim lptim?

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

led-off lp-blink
