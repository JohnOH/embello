\ pulse width modulation
\ needs timer-stm32f1.fs

\ the following pins are supported for PWM setup:
\   TIM1:   PA8  PA9  PA10 PA11
\   TIM2:   PA0  PA1  PA2  PA3
\   TIM3:   PA6  PA7  PB0  PB1
\   TIM4:   PB6  PB7  PB8  PB9

: p2tim ( pin -- n )  \ convert pin to timer (1..4)
  case
    dup PA4 <                ?of 2 endof
    dup PB1 >                ?of 4 endof
    dup PA7 > over PB0 < and ?of 1 endof
    dup PB6 <                ?of 3 endof
  endcase ;

: p2cmp ( pin -- n )  \ convert pin to output comp-reg# - 1 (0..3)
  dup
  case
    dup PA4 <                ?of 0 endof
    dup PB1 >                ?of 2 endof
    dup PA7 > over PB0 < and ?of 0 endof
    dup PB6 <                ?of 2 endof
  endcase
  + 3 and ;

\ : t dup p2tim . p2cmp . ." : " ;
\ : u                             \ expected output:
\   cr PA8 t PA9 t PA10 t PA11 t  \  1 0 : 1 1 : 1 2 : 1 3 :
\   cr PA0 t PA1 t PA2  t PA3  t  \  2 0 : 2 1 : 2 2 : 2 3 :
\   cr PA6 t PA7 t PB0  t PB1  t  \  3 0 : 3 1 : 3 2 : 3 3 :
\   cr PB6 t PB7 t PB8  t PB9  t  \  4 0 : 4 1 : 4 2 : 4 3 :
\ ;
\ u

: +pwm ( div pin -- )  \ set up PWM for a pin, with given 7200 Hz divider
  >r  OMODE-AF-PP r@ io-mode!
  1- 16 lshift 10000 or  r@ p2tim +timer
  $78 r@ p2cmp 1 and 8 * lshift ( $0078 or $7800)
  r@ p2tim timer-base $18 + r@ p2cmp 2 and 2* + bis!
  4 bit r> p2tim timer-base $20 + bis! ;

: -pwm ( pin -- )  \ disable PWM, but leave timer running
  p2tim timer-base $20 + 4 bit swap bic! ;

: pwm ( u pin -- )  \ set pwm rate, 0 = full off, 10000 = full on
  dup p2cmp cells swap p2tim timer-base + $34 + !  \ save to CCR1..4
;
