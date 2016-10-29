\ pwm experiment
\ needs core.fs
cr cr reset

\ include ../flib/timer-stm32l0.fs
\ include ../flib/pwm-stm32l0.fs

\ pins PA0..3 are assumed to have LEDs attached

$00002222 gpio-base gpio.afrl + !
omode-pp pa0 io-mode!
omode-pp pa1 io-mode!
omode-pp pa2 io-mode!
omode-pp pa3 io-mode!

: go
     2 PA0 +pwm  \ 2 hz
     2 PA1 +pwm  \ 2 hz
     2 PA2 +pwm  \ 2 hz
     2 PA3 +pwm  \ 2 hz
   500 PA0 pwm
  3500 PA1 pwm
  6500 PA2 pwm
  9500 PA3 pwm
;

go
