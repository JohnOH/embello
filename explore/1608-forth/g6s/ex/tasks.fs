\ try out the multitasker on a HyTiny with some LEDs attached

singletask forgetram

PA12 constant LED1
PA11 constant LED2
PA8  constant LED3
PB2  constant LED4

: setup
  OMODE-PP LED1 io-mode!
  OMODE-PP LED2 io-mode!
  OMODE-PP LED3 io-mode!
  OMODE-PP LED4 io-mode!
  2 0 do
    LED1 iox! 100 ms LED1 iox!
    LED2 iox! 100 ms LED2 iox!
    LED3 iox! 100 ms LED3 iox!
    LED4 iox! 100 ms LED4 iox!
  loop ;

task: blinker1
task: blinker2
task: blinker3
task: blinker4

: blink1& ( -- )
  blinker1 activate
  begin
    LED1 iox!   \ toggle LED1
    200 ms      \ wait 200 ms
  again ;

: blink2& ( -- )
  blinker2 activate
  begin
    LED2 iox!   \ toggle LED2
    300 ms      \ wait 300 ms
  again ;

: blink3& ( -- )
  blinker3 activate
  begin
    LED3 iox!   \ toggle LED3
    500 ms      \ wait 500 ms
  again ;

: blink4& ( -- )
  blinker4 activate
  begin
    LED4 iox!   \ toggle LED4
    700 ms      \ wait 700 ms
  again ;

setup multitask

\ activate all blinker tasks and show the task list
blink1& blink2& blink3& blink4& tasks
