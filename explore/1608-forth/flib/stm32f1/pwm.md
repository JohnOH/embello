# Pulse Width Modulation

* Code: `pwm-stm32fs1.fs`
* Needs `io-stm32f1.fs`, `timer-stm32f1.fs`

The following pins are supported for PWM setup on STM32F1xx:

    TIM1:   PA8  PA9  PA10 PA11
    TIM2:   PA0  PA1  PA2  PA3
    TIM3:   PA6  PA7  PB0  PB1
    TIM4:   PB6  PB7  PB8  PB9

Pins sharing a timer will run at the same repetition rate.  
Repetition rates which are a divisor of 7200 will be exact.

### API

```forth
: pwm-init ( hz pin -- )  \ set up PWM for pin, using specified repetition rate
: pwm-deinit ( pin -- )  \ disable PWM, but leave timer running
: pwm ( u pin -- )  \ set pwm rate, 0 = full off, 10000 = full on
```

### Examples

LED on PA1, blinking at 1 Hz:

```forth
    1 PA1 pwm-init  \ initialise, 1 Hz
    0 PA1 pwm       \ off
   10 PA1 pwm       \ brief blip
 5000 PA1 pwm       \ blink 50%
10000 PA1 pwm       \ full on
```

LED on PA9, dimmable:

```forth
  120 PA9 pwm-init  \ initialise, 120 Hz
    0 PA9 pwm       \ off
   10 PA9 pwm       \ very dim
 5000 PA9 pwm       \ half dimmed
10000 PA9 pwm       \ full on
```

Servo on PB0:

```forth
   50 PB0 pwm-init  \ initialise, 50 Hz = 20 ms cycle
  500 PB0 pwm       \ minimum position, 500x 2 µs = 1 ms pulses
  750 PB0 pwm       \ centre position, 750x 2 µs = 1.5 ms pulses
 1000 PB0 pwm       \ maximum position, 1000x 2 µs = 2 ms pulses
```

### Caveat

The PWM hardware cannot keep the pin completely off with a "0" arg, there's
always a minimal 0.01% duty cycle blip. To have a PWM implementation which
really does completely turn off, you can re-define `pwm` as follows (thanks to
`@tht` for this trick):

```
: pwm ( u pin -- )  \ Make sure pwm is completly off at 0
  over if
    \ enable pwm output on pin
    dup dup p2cmp 4 * bit swap p2tim timer-base $20 + bis!
    pwm
  else
    pwm-deinit drop
  then
; 
```
