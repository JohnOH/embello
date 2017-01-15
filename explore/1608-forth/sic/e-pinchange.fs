compiletoram

: -swd AFIO-MAPR @ %111 24 lshift bic 26 bit or AFIO-MAPR ! ;
-swd

\ kinda WORKING!
\ see here for pinchange interrupt:
\ https://github.com/jeelabs/embello/blob/master/explore/1608-forth/oxs/l

IMODE-PULL PB3 io-mode! PB3 ios!
IMODE-PULL PB4 io-mode! PB4 ios!

\ : toggle-heater ( -- )
\   out-override @ -1  <> IF
\     ." SWITCHING ON"
\     auto
\   ELSE
\     ." SWITCHING OFF"
\     0 manual
\   THEN
\ ;

: toggle-heater ( -- ) ." PRESSED" CR ;

$E000E100 constant NVIC-EN0R \ IRQ 0 to 31 Set Enable Register
AFIO $8 + constant AFIO-EXTICR1
AFIO $C + constant AFIO-EXTICR2

$40010400 constant EXTI
    EXTI $00 + constant EXTI-IMR
    EXTI $08 + constant EXTI-RTSR
    EXTI $0C + constant EXTI-FTSR
    EXTI $14 + constant EXTI-PR


0 variable debounce
: ext3-tick ( -- )  \ interrupt handler for EXTI3
  ." PB3" CR
  3 bit EXTI-PR !
  millis debounce @ - 200 > IF
    toggle-heater
    millis debounce !
  THEN
;


['] ext3-tick irq-exti3 !     \ install interrupt handler EXTI 3
9 bit NVIC-EN0R bis!  \ enable EXTI3 interrupt 9
%0001 12 lshift AFIO-EXTICR1 bis!  \ select P<B>4
4 bit EXTI-IMR bis!  \ enable PB<4>
4 bit EXTI-RTSR bis!  \ trigger on PB<4> rising edge
