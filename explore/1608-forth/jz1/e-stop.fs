\ stop mode experiment
\ needs core.fs

cr cr reset
cr

include ../flib/sleep-stm32l0.fs

     RCC $2C + constant RCC-IOPENR
     $40015804 constant DBG-CR

: lp-blink ( -- )
  2.1MHz  1000 systick-hz
\ 11 bit PWR-CR bis!                  \ 1.2V, range 3
\ begin 4 bit PWR-CSR bit@ not until  \ wait for !VOSF
  begin
    stop1s stop1s stop1s stop1s stop1s
    stop1s stop1s stop1s stop1s stop1s
    led-on 10 ms led-off
  again ;

[IFDEF] rf69-init  rf69-init rf-sleep  [THEN]
+lptim lptim?

\ this s(h)aves 0.4 µA
IMODE-ADC PA0  io-mode!
IMODE-ADC PA1  io-mode!
IMODE-ADC PA2  io-mode!
IMODE-ADC PA3  io-mode!
IMODE-ADC PA4  io-mode!
IMODE-ADC PA5  io-mode!
IMODE-ADC PA6  io-mode!
IMODE-ADC PA7  io-mode!
IMODE-ADC PA8  io-mode!
IMODE-ADC PA11 io-mode!
IMODE-ADC PA12 io-mode!
IMODE-ADC PA13 io-mode!
IMODE-ADC PA14 io-mode!
IMODE-ADC PB0  io-mode!
IMODE-ADC PB1  io-mode!
IMODE-ADC PB3  io-mode!
IMODE-ADC PB4  io-mode!
IMODE-ADC PB5  io-mode!
IMODE-ADC PB6  io-mode!
IMODE-ADC PB7  io-mode!
IMODE-ADC PC14 io-mode!
IMODE-ADC PC15 io-mode!

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
( SCR         ) SCR         @ hex.

led-off lp-blink
