\ hardware timers

     $40012C00 constant TIM1  
     $40013400 constant TIM8  
     $40014C00 constant TIM9  
     $40015000 constant TIM10  
     $40015400 constant TIM11  

     $40000000 constant TIM2  
     $40000400 constant TIM3  
     $40000800 constant TIM4  
     $40000C00 constant TIM5  
     $40001000 constant TIM6  
     $40001400 constant TIM7  
     $40001800 constant TIM12  
     $40001C00 constant TIM13  
     $40002000 constant TIM14  

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

create timer-table
  111 c,  \ TIM1  APB2
  0   c,  \ TIM2  APB1
  1   c,  \ TIM3  APB1
  2   c,  \ TIM4  APB1
  3   c,  \ TIM5  APB1
  4   c,  \ TIM6  APB1
  5   c,  \ TIM7  APB1
  113 c,  \ TIM8  APB2
  119 c,  \ TIM9  APB2
  120 c,  \ TIM10 APB2
  121 c,  \ TIM11 APB2
  6   c,  \ TIM12 APB1
  7   c,  \ TIM13 APB1
  8   c,  \ TIM14 APB1
calign

: timer-lookup ( n - pos ) 1- timer-table + c@ ;
: timer-base ( n -- addr )  \ return base address for timer 1..14
  timer-lookup
  dup 100 < if  $400 * $40000000  else  111 - $400 * $40012C00  then + ;
: timer-enabit ( n -- bit addr )  \ return bit and enable address for timer n
  timer-lookup
  dup 100 < if  bit RCC-APB1ENR  else  111 - bit RCC-APB2ENR  then ;

: +timer3 ( u -- )  \ see timer 3 to free-running with given period
  3 timer-enabit bis!  \ TIM3EN clock enable
  TIM3-ARR !
  8 bit TIM3-DIER bis!  \ UDE
  %010 4 lshift TIM3-CR2 !  \ MMS = update
  0 bit TIM3-CR1 !  \ CEN
;

: +timer6 ( u -- )  \ see timer 6 to free-running with given period
  6 timer-enabit bis!  \ TIM6EN clock enable
  TIM6-ARR !
  8 bit TIM6-DIER bis!  \ UDE
  %010 4 lshift TIM6-CR2 !  \ MMS = update
  0 bit TIM6-CR1 !  \ CEN
;
