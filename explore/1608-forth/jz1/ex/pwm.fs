\ pwm experiment
\ needs core.fs
cr cr reset

\ include ../flib/stm32l0/timer.fs
\ include ../flib/stm32l0/pwm.fs

\ pins PA0..3 are assumed to have LEDs attached

\ FIXME set alt function #2 on all PWM pins, should be moved inside pwm driver
$00002222 PA0 io-base GPIO.AFRL + !

\ various duty cycles at 2 Hz
2 PA0 pwm-init   500 PA0 pwm
2 PA1 pwm-init  3500 PA1 pwm
2 PA2 pwm-init  6500 PA2 pwm
2 PA3 pwm-init  9500 PA3 pwm
