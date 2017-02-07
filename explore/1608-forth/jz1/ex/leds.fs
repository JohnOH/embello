\ turn on all I/O pins on the main header in sequence
\ needs core.fs
cr cr reset

\ all relevant pins are assumed to have LEDs attached

: set-out ( pin -- )  OMODE-PP over io-mode!  ioc! ;

100 constant N

: toggle ( pin -- )  dup ios!  100 ms  ioc! ;

: go
  begin
    PA0  toggle
    PA1  toggle
    PA2  toggle
    PA3  toggle
    PA11 toggle
    PA12 toggle
    PA13 toggle
    PA14 toggle
    PB3  toggle
    PB4  toggle
    PB5  toggle
    PB6  toggle
    PB7  toggle
  key? until ;

PA0  set-out
PA1  set-out
PA2  set-out
PA3  set-out
PA11 set-out
PA12 set-out
PA13 set-out
PA14 set-out
PB3  set-out
PB4  set-out
PB5  set-out
PB6  set-out
PB7  set-out

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
