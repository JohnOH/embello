\ standby mode experiment
\ needs core.fs
cr cr reset
cr

$40007000 constant PWR
      PWR $0 + constant PWR-CR
      PWR $4 + constant PWR-CSR

$40010400 constant EXTI
     EXTI $00 + constant EXTI-IMR
     EXTI $04 + constant EXTI-EMR
\    EXTI $08 + constant EXTI-RTSR
\    EXTI $0C + constant EXTI-FTSR
\    EXTI $10 + constant EXTI-SWIER
     EXTI $14 + constant EXTI-PR

\ see https://developer.arm.com/docs/dui0662/latest/4-cortex-m0-peripherals/
\                       43-system-control-block/436-system-control-register
$E000ED10 constant SCR

: wfe ( -- ) [ $BF20 h, ] inline ; \ WFE Opcode, enters sleep mode

: standby ( -- )
  1 bit PWR-CR bis!                     \ set PDDS for standby mode
\ 0 bit PWR-CR bis!                     \ set LPSDSR
\ 29 bit EXTI-IMR bic!                  \ clear IM29
  29 bit EXTI-EMR bis!                  \ set EM29
\ -1 EXTI-PR !                          \ clear all pending
  2 bit SCR bis!                        \ set SLEEPDEEP
;

( clock-hz ) clock-hz @ .
( PWR-CR ) PWR-CR @ hex.
( PWR-CSR ) PWR-CSR @ hex.
( RCC-CR ) RCC-CR @ hex.
( RCC-CCIPR ) RCC-CCIPR @ hex.
( EXTI-IMR ) EXTI-IMR @ hex.
( EXTI-EMR ) EXTI-EMR @ hex.
( EXTI-PR ) EXTI-PR @ hex.

rf69-init rf-sleep led-off
0 RCC-CCIPR !

standby wfe
