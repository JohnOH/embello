\ basic quadrature pulse decoder, unattended & ultra-low power mode

<<<core>>>
compiletoflash

PA3 constant ENC-A
PA5 constant ENC-B
PA4 constant ENC-C  \ common

$E000E100 constant NVIC-EN0R \ IRQ 0 to 31 Set Enable Register

AFIO $8 + constant AFIO-EXTICR1
AFIO $C + constant AFIO-EXTICR2

\ some of these are already defined in flib/stm32l0/sleep.fs
\ $40010400 constant EXTI
\   EXTI $00 + constant EXTI-IMR
    EXTI $08 + constant EXTI-RTSR
    EXTI $0C + constant EXTI-FTSR
\   EXTI $14 + constant EXTI-PR

1000 variable counter

: encA-tick ( -- )  \ interrupt handler for EXTI2_3
  ENC-B io@ if 1 else -1 then counter +!  3 bit EXTI-PR ! ;

: encB-tick ( -- )  \ interrupt handler for EXTI4-15
  ENC-A io@ if -1 else 1 then counter +!  5 bit EXTI-PR ! ;

: count-pulses ( -- )  \ set up and start the external interrupts
     ['] encA-tick irq-exti2_3 !     \ install interrupt handler EXTI 2-3
    ['] encB-tick irq-exti4_15 !     \ install interrupt handler EXTI 4-15

               6 bit NVIC-EN0R bis!  \ enable EXTI2_3 interrupt 6
  %0001 12 lshift AFIO-EXTICR1 bis!  \ select P<A>3
                3 bit EXTI-IMR bis!  \ enable PA<3>
               3 bit EXTI-FTSR bis!  \ trigger on PA<3> falling edge

               7 bit NVIC-EN0R bis!  \ enable EXTI4_15 interrupt 7
   %0001 4 lshift AFIO-EXTICR2 bis!  \ select P<A>5
                5 bit EXTI-IMR bis!  \ enable PA<5>
               5 bit EXTI-FTSR bis!  \ trigger on PA<5> falling edge
;

: read-enc
  IMODE-HIGH ENC-A io-mode!
  IMODE-HIGH ENC-B io-mode!
  OMODE-PP   ENC-C io-mode!  ENC-C ioc!
  rf-init
  led-off 2.1MHz only-msi 1000 systick-hz lptim-init
  count-pulses

  begin
    7 <pkt counter @ +pkt pkt>rf rf-sleep
    stop10s
  again ;

: init init unattended read-enc ;
