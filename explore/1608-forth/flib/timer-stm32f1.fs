\ hardware timers

$40000400 constant TIM3
    TIM3 $00 + constant TIM3-CR1
    TIM3 $04 + constant TIM3-CR2
    TIM3 $0C + constant TIM3-DIER
    TIM3 $2C + constant TIM3-ARR

$40001000 constant TIM6
    TIM6 $00 + constant TIM6-CR1
    TIM6 $04 + constant TIM6-CR2
    TIM6 $0C + constant TIM6-DIER
    TIM6 $2C + constant TIM6-ARR

: tim3-init ( u -- )  \ see timer 3 to free-running with given period
  1 bit RCC-APB1ENR bis!  \ TIM3EN clock enable
  TIM3-ARR !
  8 bit TIM3-DIER bis!  \ UDE
  %010 4 lshift TIM3-CR2 !  \ MMS = update
  0 bit TIM3-CR1 !  \ CEN
;

: tim6-init ( u -- )  \ see timer 6 to free-running with given period
  4 bit RCC-APB1ENR bis!  \ TIM6EN clock enable
  TIM6-ARR !
  8 bit TIM6-DIER bis!  \ UDE
  %010 4 lshift TIM6-CR2 !  \ MMS = update
  0 bit TIM6-CR1 !  \ CEN
;
