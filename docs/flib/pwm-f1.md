# Pulse Width Modulation

[code]: stm32f1/pwm.fs (io timer)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f1/pwm.fs">stm32f1/pwm.fs</a>
* Needs: io, timer

The following pins are supported for PWM setup on STM32F1xx:

    TIM1:   PA8  PA9  PA10 PA11
    TIM2:   PA0  PA1  PA2  PA3
    TIM3:   PA6  PA7  PB0  PB1
    TIM4:   PB6  PB7  PB8  PB9

Pins sharing a timer will run at the same repetition rate.  
Repetition rates which are a divisor of 7200 will be exact.

### API

[defs]: <> (pwm-init pwm-deinit pwm)
```
: pwm-init ( hz pin -- )  \ set up PWM for pin, using specified repetition rate
: pwm-deinit ( pin -- )  \ disable PWM, but leave timer running
: pwm ( u pin -- )  \ set pwm rate, 0 = full off, 10000 = full on
```

### Examples

LED on PA1, blinking at 1 Hz:

```
    1 PA1 pwm-init  \ initialise, 1 Hz
    0 PA1 pwm       \ off
   10 PA1 pwm       \ brief blip
 5000 PA1 pwm       \ blink 50%
10000 PA1 pwm       \ full on
```

LED on PA9, dimmable:

```
  120 PA9 pwm-init  \ initialise, 120 Hz
    0 PA9 pwm       \ off
   10 PA9 pwm       \ very dim
 5000 PA9 pwm       \ half dimmed
10000 PA9 pwm       \ full on
```

Servo on PB0:

```
   50 PB0 pwm-init  \ initialise, 50 Hz = 20 ms cycle
  500 PB0 pwm       \ minimum position, 500x 2 µs = 1 ms pulses
  750 PB0 pwm       \ centre position, 750x 2 µs = 1.5 ms pulses
 1000 PB0 pwm       \ maximum position, 1000x 2 µs = 2 ms pulses
```
