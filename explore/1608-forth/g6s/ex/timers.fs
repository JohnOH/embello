\ try out the "timed" package on a HyTiny with some LEDs attached

singletask forgetram

include ../flib/any/timed.fs

PA12 constant LED1
PA11 constant LED2
PA8  constant LED3
PB2  constant LED4

: setup
  OMODE-PP LED1 io-mode!
  OMODE-PP LED2 io-mode!
  OMODE-PP LED3 io-mode!
  OMODE-PP LED4 io-mode! ;

: blink1 ( -- )  LED1 iox! ;   \ toggle LED1
: blink2 ( -- )  LED2 iox! ;   \ toggle LED2
: blink3 ( -- )  LED3 iox! ;   \ toggle LED3
: blink4 ( -- )  LED4 iox! ;   \ toggle LED4

setup
timed-init

' blink1 200 0 call-every
' blink2 300 1 call-every
' blink3 500 2 call-every
' blink4 700 3 call-every
