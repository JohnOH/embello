\ base definitions for STM32F103
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs io.fs

\ good to know: access to STM32F1's serial h/w id
\ $1FFFF7E8 constant ID1
\ $1FFFF7EC constant ID2
\ $1FFFF7F0 constant ID3

: io.all ( -- )  \ display all the readable GPIO registers
  5 0 do i 0 io io. loop ;

0 0  io constant PA0      1 0  io constant PB0      3 0  io constant PD0
0 1  io constant PA1      1 1  io constant PB1      3 1  io constant PD1
0 2  io constant PA2      1 2  io constant PB2
0 3  io constant PA3      1 3  io constant PB3
0 4  io constant PA4      1 4  io constant PB4
0 5  io constant PA5      1 5  io constant PB5
0 6  io constant PA6      1 6  io constant PB6
0 7  io constant PA7      1 7  io constant PB7
0 8  io constant PA8      1 8  io constant PB8
0 9  io constant PA9      1 9  io constant PB9
0 10 io constant PA10     1 10 io constant PB10
0 11 io constant PA11     1 11 io constant PB11
0 12 io constant PA12     1 12 io constant PB12
0 13 io constant PA13     1 13 io constant PB13     2 13 io constant PC13
0 14 io constant PA14     1 14 io constant PB14     2 14 io constant PC14
0 15 io constant PA15     1 15 io constant PB15     2 15 io constant PC15

$40010000 constant AFIO
     AFIO $4 + constant AFIO-MAPR

$40013800 constant USART1
   USART1 $8 + constant USART1-BRR

$40021000 constant RCC
     RCC $00 + constant RCC-CR
     RCC $04 + constant RCC-CFGR
     RCC $10 + constant RCC-APB1RSTR
     RCC $18 + constant RCC-APB2ENR
     RCC $1C + constant RCC-APB1ENR

$40022000 constant FLASH
    FLASH $0 + constant FLASH-ACR

: -jtag ( -- )  \ disable JTAG on PB3 PB4 PA15
  25 bit AFIO-MAPR bis! ;

\ adjusted for STM32F103 @ 72 MHz (original STM32F100 by Igor de om1zz, 2015)

12000000 variable clock-hz

: 72MHz ( -- )  \ set the main clock to 72 MHz, keep baud rate at 115200
  2 FLASH-ACR bis!                \ two flash mem wait states
  16 bit RCC-CR bis!              \ set HSEON
  begin 17 bit RCC-CR bit@ until  \ wait for HSERDY
  1 16 lshift                     \ HSE clock is 8 MHz Xtal source for PLL
  7 18 lshift or                  \ PLL factor: 8 MHz * 9 = 72 MHz = HCLK
  4  8 lshift or                  \ PCLK1 = HCLK/2
  3 14 lshift or                  \ ADCPRE = PCLK2/8
            2 or  RCC-CFGR !      \ PLL is the system clock
  24 bit RCC-CR bis!              \ set PLLON
  begin 25 bit RCC-CR bit@ until  \ wait for PLLRDY
  72000000 clock-hz !
  $271 USART1-BRR ! \ set baud rate divider for 115200 Baud at PCLK2=72MHz
;

: systick ( ticks -- )  \ enable systick interrupt
  1- $E000E014 !  \ How many ticks between interrupts ?
  7 $E000E010 !   \ Enable the systick interrupt.
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
