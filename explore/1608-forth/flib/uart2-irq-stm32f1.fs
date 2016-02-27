\ interrupt-based USART2 with input ring buffer
\ needs ring.fs
\ needs uart2-stm32f1.fs

\ these definitions redefine three words in uart2-stm32f1.fs

128 4 + buffer: uart-ring

: uart-irq-handler ( -- )  \ handle the USART receive interrupt
  USART2-DR @  \ will drop input when there is no room left
  uart-ring dup ring? if >ring else 2drop then ;

$E000E104 constant NVIC-EN1.R       \ IRQ 32 to 63 Set Enable Register
         6 bit constant INT38       \ USART2 Interrupt 38

5 bit constant RXNEIE

: uart-init ( -- )  \ redefined
\ initialise the USART2, using a receive ring buffer
  uart-init
  uart-ring 128 init-ring
  ['] uart-irq-handler irq-usart2 !
  INT38 NVIC-EN1.R !
  RXNEIE USART2-CR1 bis! ;

: uart-key? ( -- f )  \ redefined
  uart-ring ring# 0<> ;
: uart-key ( -- c )  \ redefined
  begin uart-key? until  uart-ring ring> ;
