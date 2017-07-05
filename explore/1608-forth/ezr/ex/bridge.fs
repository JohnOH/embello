\ Serial USB <=> USART2 bridge

compiletoram? [if]  forgetram  [then]

PB0 constant XIN
PB1 constant RST
PB4 constant ZCL
PB5 constant ZDA

: ez80-8MHz ( -- )
  7200 XIN pwm-init   \ first set up pwm correctly
  8 3 timer-init      \ then mess with the timer divider, i.e. ÷9
  9996 XIN pwm ;      \ finally, set the pwm to still toggle

: delay 10 0 do loop ;
: zcl-lo  delay ZCL ioc! delay ;
: zcl-hi  delay ZCL ios! delay ;

: zdi! ( f -- )  zcl-lo  ZDA io!  zcl-hi  ZDA ios! ;

: zdi-start ( u -- )
  ( zcl-hi ) ZDA ioc!
  OMODE-PP ZDA io-mode!
  7 0 do
    dup $40 and zdi!  shl
  loop  drop ;

: zdi> ( addr -- val )
  zdi-start  1 zdi!  1 zdi!
  OMODE-OD ZDA io-mode!
  0  8 0 do
    zcl-lo  zcl-hi
    shl  ZDA io@ 1 and or
  loop
  zcl-lo ZDA ios! zcl-hi ;

: >zdi ( val addr -- )
  zdi-start  0 zdi!  1 zdi!
  8 0 do
    dup $80 and zdi!  shl
  loop  drop
  zcl-lo ZDA ios! zcl-hi
  OMODE-OD ZDA io-mode! ;

: zdi-init ( -- )
  RST ios!  OMODE-OD RST io-mode!
  ZDA ios!  OMODE-OD ZDA io-mode!
  ZCL ios!  OMODE-PP ZCL io-mode!
  ez80-8MHz  $80 $11 >zdi ;

task: uart-task

: uart-reader&
  uart-task background
  begin
    begin uart-irq-key? while uart-irq-key emit repeat
    stop
  again ;

: run
  zdi-init
  ." Connecting to USART2..." cr
  uart-irq-init 19200 uart-baud
  [: uart-irq-handler uart-task wake ;] irq-usart2 !
  multitask uart-reader&
  begin
    key? if key uart-emit then
  again ;

\ zdi-init y
\ run
