\ hardware timers

$40001000 constant TIM6
    TIM6 $00 + constant TIM6-CR1
    TIM6 $04 + constant TIM6-CR2
    TIM6 $0C + constant TIM6-DIER
    TIM6 $2C + constant TIM6-ARR

: tim6-init ( u -- )  \ see timer 6 to free-running with given period
  4 bit RCC-APB1ENR bis!  \ TIM6EN clock enable
  TIM6-ARR !
  8 bit TIM6-DIER bis!  \ UDE
  %010 4 lshift TIM6-CR2 !  \ MMS = update
  0 bit TIM6-CR1 !  \ CEN
;
