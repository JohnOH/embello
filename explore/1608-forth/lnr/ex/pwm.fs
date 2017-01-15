\ pwm experiment
\ needs core.fs
cr cr reset

\ pins PA0..2 are assumed to have LEDs attached

\ FIXME set alt function #2 on all PWM pins, should be moved inside pwm driver
$00000222 PA0 io-base GPIO.AFRL + !

PA0 constant RED
PA1 constant GREEN
PA2 constant BLUE

\ various duty cycles at 100 Hz
100 RED   pwm-init  5000 RED   pwm
100 GREEN pwm-init  3000 GREEN pwm
100 BLUE  pwm-init  2000 BLUE  pwm
