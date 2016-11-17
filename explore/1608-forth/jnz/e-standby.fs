\ standby mode experiment

cr cr reset
\ <<<board>>>
\ compiletoflash

\ include ../flib/sleep-stm32l0.fs

     RCC $2C + constant RCC-IOPENR
     $40015804 constant DBG-CR

: standby ( -- )
  2.1MHz  1000 systick-hz
\ 11 bit PWR-CR bis!    \ 1.2V, range 3
\ PWR-CR      @ hex.
\ PWR-CSR     @ hex.
\ begin 4 bit PWR-CSR bit@ not until  \ wait for !VOSF
\ PWR-CR      @ hex.
\ PWR-CSR     @ hex.
  only-msi
\ 0 RCC-APB2ENR !       \ disable USART1
\ %1 RCC-IOPENR !       \ disable all GPIO clocks, except GPIOA
\ 9 bit PWR-CR bis!     \ set ULP
  1 bit PWR-CR bis!     \ set PDDS for standby mode
\ 0 bit PWR-CR bis!     \ set LPSDSR
\ 14 bit PWR-CR bis!    \ set LPRUN
\ 29 bit EXTI-IMR bic!  \ clear IM29
  29 bit EXTI-EMR bis!  \ set EM29
\ -1 EXTI-PR !          \ clear all pending
  2 bit SCR bis!        \ set SLEEPDEEP
  wfe                   \ enter standby mode
;

[IFDEF] rf69-init  rf69-init rf-sleep  [THEN]
\ IMODE-HIGH  PA0  io-mode!
\ IMODE-HIGH  PA4  io-mode!
\ IMODE-LOW   PA5  io-mode!
\ IMODE-LOW   PA6  io-mode!
\ IMODE-LOW   PA7  io-mode!
\ IMODE-LOW   PA8  io-mode!
\ IMODE-HIGH  PA9  io-mode!
\ IMODE-HIGH  PA10 io-mode!
\ IMODE-LOW   PA11 io-mode!
\ IMODE-LOW   PA12 io-mode!
\ IMODE-LOW   PB0  io-mode!
\ IMODE-LOW   PB1  io-mode!
\ IMODE-HIGH  PA15 io-mode!


\ 2.1MHz  1000 systick-hz
\ $1800 PWR-CR !        \ 1.2V, range 3
\ 0 bit PWR-CR bis!     \ set LPSDSR
\ 9 bit PWR-CR bis!     \ set ULP
\ 14 bit PWR-CR bis!    \ set LPRUN

( clock-hz    ) clock-hz    @ .
( PWR-CR      ) PWR-CR      @ hex.
( PWR-CSR     ) PWR-CSR     @ hex.
( RCC-CR      ) RCC-CR      @ hex.
( RCC-CSR     ) RCC-CSR     @ hex.
( RCC-CCIPR   ) RCC-CCIPR   @ hex.
( RCC-AHBENR  ) RCC-AHBENR  @ hex.
( RCC-APB1ENR ) RCC-APB1ENR @ hex.
( RCC-APB2ENR ) RCC-APB2ENR @ hex.
( RCC-IOPENR  ) RCC-IOPENR  @ hex.
( EXTI-IMR    ) EXTI-IMR    @ hex.
( EXTI-EMR    ) EXTI-EMR    @ hex.
( EXTI-PR     ) EXTI-PR     @ hex.
( DBG-CR      ) DBG-CR      @ hex.
( SCR         ) SCR         @ hex.

led-off standby led-on
