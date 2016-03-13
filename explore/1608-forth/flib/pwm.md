# Pulse Width Modulation

* Code: `pwm-stm32fs1.fs`
* Needs `io-stm32f1.fs` + `timer-stm32f1.fs`

The following pins are supported for PWM setup:

    TIM1:   PA8  PA9  PA10 PA11
    TIM2:   PA0  PA1  PA2  PA3
    TIM3:   PA6  PA7  PB0  PB1
    TIM4:   PB6  PB7  PB8  PB9

Pins sharing a timer will run at the same repetition rate.

### API

```forth
: +pwm ( div pin -- )  \ set up PWM for a pin, with given 7200 Hz divider
: -pwm ( pin -- )  \ disable PWM, but leave timer running
: pwm ( u pin -- )  \ set pwm rate, 0 = full off, 10000 = full on
```

### Examples

LED on PA1, blinking at 1 Hz:

```forth
 7200 PA1 +pwm      \ initialise, 7200/7200 = 1 Hz
    0 PA1 pwm       \ off
   10 PA1 pwm       \ brief blip
 5000 PA1 pwm       \ blink 50%
10000 PA1 pwm       \ full on
```

LED on PA9, dimmable:

```forth
   60 PA9 +pwm      \ initialise, 7200/60 = 120 Hz
    0 PA9 pwm       \ off
   10 PA9 pwm       \ very dim
 5000 PA9 pwm       \ half dimmed
10000 PA9 pwm       \ full on
```

Servo on PB0:

```forth
  144 PB0 +pwm      \ initialise, 7200/144 = 50 Hz = 20 ms
  500 PB0 pwm       \ minimum position, 500x 2 µs = 1 ms pulse
  750 PB0 pwm       \ centre position, 750x 2 µs = 1.5 ms pulse
 1000 PB0 pwm       \ maximum position, 1000x 2 µs = 2 ms pulse
```
