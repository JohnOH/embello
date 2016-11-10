\ standby mode experiment
\ needs core.fs
cr cr reset
cr

\ include ../flib/sleep-stm32l0.fs

: standby ( -- )
  1 bit PWR-CR bis!     \ set PDDS for standby mode
\ 0 bit PWR-CR bis!     \ set LPSDSR
\ 29 bit EXTI-IMR bic!  \ clear IM29
  29 bit EXTI-EMR bis!  \ set EM29
\ -1 EXTI-PR !          \ clear all pending
  2 bit SCR bis!        \ set SLEEPDEEP
  wfe                   \ enter standby mode
;

( clock-hz ) clock-hz @ .
( PWR-CR ) PWR-CR @ hex.
( PWR-CSR ) PWR-CSR @ hex.
( RCC-CR ) RCC-CR @ hex.
( RCC-CCIPR ) RCC-CCIPR @ hex.
( EXTI-IMR ) EXTI-IMR @ hex.
( EXTI-EMR ) EXTI-EMR @ hex.
( EXTI-PR ) EXTI-PR @ hex.

rf69-init rf-sleep
led-off 2.1MHz

\ this causes folie to timeout on include matching, yet still starts running
1234 ms only-msi standby
