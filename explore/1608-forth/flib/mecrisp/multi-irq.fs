\ cooperative multitasking w/ interrupts

include multi.fs

\ Most of multi.fs is interrupt safe.
\ However, if you use an interrupt to wake up your task, this code
\
\   need-some-state? if
\     setup-statechange-irq stop
\   then
\
\ is incorrect: when the interrupt occurs after setting
\ up the interrupt but before stopping your task, it won't wake up.
\ 
\ The correct way to handle this is to prepare for stopping
\ *before* checking the condition:
\
\   need-some-state? if
\     will-stop setup-statechange-irq pause
\   then

: will-stop ( -- ) \ Stop current task at next pause
  false task-state ! inline ;
: wont-stop ( -- ) \ Do not stop current task at next pause
  true task-state ! inline ;

