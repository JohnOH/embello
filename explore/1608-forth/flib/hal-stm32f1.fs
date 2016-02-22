\ base definitions for STM32F103
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs io.fs

: io.all ( -- )  \ display all the readable GPIO registers
  5 0 do i 0 io io. loop ;

0 0  io constant PA0
0 1  io constant PA1
0 2  io constant PA2
0 3  io constant PA3
0 4  io constant PA4
0 5  io constant PA5
0 6  io constant PA6
0 7  io constant PA7
0 8  io constant PA8
0 9  io constant PA9
0 10 io constant PA10
0 11 io constant PA11
0 12 io constant PA12
0 13 io constant PA13
0 14 io constant PA14
0 15 io constant PA15

1 0  io constant PB0
1 1  io constant PB1
1 2  io constant PB2
1 3  io constant PB3
1 4  io constant PB4
1 5  io constant PB5
1 6  io constant PB6
1 7  io constant PB7
1 8  io constant PB8
1 9  io constant PB9
1 10 io constant PB10
1 11 io constant PB11
1 12 io constant PB12
1 13 io constant PB13
1 14 io constant PB14
1 15 io constant PB15

2 13 io constant PC13
2 14 io constant PC14
2 15 io constant PC15

3 0  io constant PD0
3 1  io constant PD1

$40010000 constant AFIO
     AFIO $4 + constant AFIO-MAPR

$40013800 constant USART1
   USART1 $8 + constant USART1-BRR

$40021000 constant RCC
      RCC $0 + constant RCC-CR
   1 24 lshift constant PLLON
   1 25 lshift constant PLLRDY
   1 16 lshift constant HSEON
   1 17 lshift constant HSERDY
      RCC $4 + constant RCC-CFGR
   1 16 lshift constant PLLSRC

$40022000 constant FLASH
    FLASH $0 + constant FLASH-ACR

: -jtag ( -- )  \ disable JTAG on PB3 PB4 PA15
  1 25 lshift AFIO-MAPR bis! ;

\ adjusted for STM32F103 @ 72 MHz (original STM32F100 by Igor de om1zz, 2015)

12000000 variable clock-hz

: 72MHz ( -- )  \ set the main clock to 72 MHz, keep baud rate at 115200
  2 FLASH-ACR bis!                \ two flash mem wait states
  HSEON RCC-CR bis!               \ switch HSE ON
  begin HSERDY RCC-CR bit@ until  \ wait for HSE to be ready
  PLLSRC                          \ HSE clock is 8 MHz Xtal source
  7 18 lshift or                  \ PLL factor: 8 MHz * 9 = 72 MHz = HCLK
  4  8 lshift or                  \ PCLK1 = HCLK/2
  3 14 lshift or                  \ ADCPRE = PCLK2/8
            2 or  RCC-CFGR !      \ PLL is the system clock
  PLLON RCC-CR bis!               \ switch PLL ON
  begin PLLRDY RCC-CR bit@ until  \ wait for PLL to lock
  72000000 clock-hz !
  $271 USART1-BRR ! \ set baud rate divider for 115200 Baud at PCLK2=72MHz
;

: systick ( ticks -- )  \ enable systick interrupt
  1 - $E000E014 !  \ How many ticks between interrupts ?
  7 $E000E010 !    \ Enable the systick interrupt.
;

0 variable ticks

: ++ticks ( -- ) 1 ticks +! ;  \ for use as systick irq handler

: systick-hz ( u -- )  \ enable systick counter at given frequency
  ['] ++ticks irq-systick !
  clock-hz @ swap / systick ;

: list ( -- )  \ list all words in dictionary, short form
  cr dictionarystart begin
    dup 6 + ctype space
  dictionarynext until drop ;

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds begin here $3FF and while 0 h, repeat
  does>   begin dup  $3FF and while 2+   repeat  cr eraseflashfrom ;
