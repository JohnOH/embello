\ interrupt-based USART2 with input ring buffer
\ needs ring.fs
\ needs uart2-stm32f1.fs

128 4 + buffer: uart-ring

: uart-irq-handler ( -- )  \ handle the USART receive interrupt
  USART2.DR @  \ will drop input when there is no room left
  uart-ring dup ring? if >ring else 2drop then
;

$E000E104 constant NVIC.EN1.R       \ IRQ 32 to 63 Set Enable Register
    1 6 lshift constant INT38       \ USART2 Interrupt 38

1 5 lshift constant RXNEIE

: uart-irq-init ( -- )  \ initialise the USART2, using a receive ring buffer
  uart-init
  uart-ring 128 init-ring
  ['] uart-irq-handler irq-usart2 !
  INT38 NVIC.EN1.R !
  RXNEIE USART2.CR1 bis!
;

: uart-irq-key? ( -- f ) uart-ring ring# 0<> ;
: uart-irq-key ( -- c ) begin uart-irq-key? until  uart-ring ring> ;
: uart-irq-emit? ( -- f ) uart-emit? ;
: uart-irq-emit ( c -- ) uart-emit ;
