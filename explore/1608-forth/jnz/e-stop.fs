\ stop mode experiment
\ needs core.fs
cr cr reset
cr

\ include ../flib/sleep-stm32l0.fs

: lp-blink ( -- )  only-msi  begin  stop1s led iox!  key? until ;

rf69-init rf-sleep
led-off 2.1MHz
1000 systick-hz  \ FIXME if omitted, blink startup stalls for several seconds
+lptim 

( clock-hz ) clock-hz @ .
( PWR-CR ) PWR-CR @ hex.
( PWR-CSR ) PWR-CSR @ hex.
( RCC-CR ) RCC-CR @ hex.
( RCC-CSR ) RCC-CSR @ hex.
( RCC-CCIPR ) RCC-CCIPR @ hex.
( EXTI-IMR ) EXTI-IMR @ hex.
( EXTI-EMR ) EXTI-EMR @ hex.
( EXTI-PR ) EXTI-PR @ hex.

lptim?
lp-blink
