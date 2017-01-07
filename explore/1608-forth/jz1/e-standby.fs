\ standby mode experiment

cr cr reset
cr

\ include ../flib/stm32l0/sleep.fs

: standby ( -- )
  2.1MHz  1000 systick-hz  only-msi
  28 bit RCC-APB1ENR bis!  \ set PWREN
  9 bit PWR-CR bis!        \ set ULP
  2 bit PWR-CR bis!        \ set CWUF
  1 bit PWR-CR bis!        \ set PDDS for standby mode
  0 bit PWR-CR bis!        \ set LPSDSR
  2 bit SCR bis!           \ set SLEEPDEEP
  wfe                      \ enter standby mode
;

[IFDEF] rf-init  rf-init rf-sleep  [THEN]

( clock-hz    ) clock-hz    @ .
( PWR-CR      ) PWR-CR      @ hex.
( PWR-CSR     ) PWR-CSR     @ hex.
( RCC-CR      ) RCC-CR      @ hex.
( RCC-CSR     ) RCC-CSR     @ hex.
( RCC-CCIPR   ) RCC-CCIPR   @ hex.
( RCC-AHBENR  ) RCC-AHBENR  @ hex.
( RCC-APB1ENR ) RCC-APB1ENR @ hex.
( RCC-APB2ENR ) RCC-APB2ENR @ hex.
( EXTI-IMR    ) EXTI-IMR    @ hex.
( EXTI-EMR    ) EXTI-EMR    @ hex.
( EXTI-PR     ) EXTI-PR     @ hex.
( SCR         ) SCR         @ hex.

led-off standby led-on
