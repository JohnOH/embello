\ base definitions for STM32F407
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs io.fs

: flash-kb ( -- u )  \ return size of flash memory in KB
  $1FFFF7E0 h@ ;
: chipid ( -- u1 u2 u3 3 )  \ unique chip ID as N values on the stack
  $1FFFF7E8 @ $1FFFF7EC @ $1FFFF7F0 @ 3 ;
: hwid ( -- u )  \ a "fairly unique" hardware ID as single 32-bit int
  chipid 1- 0 do xor loop ;

: io.all ( -- )  \ display all the readable GPIO registers
  9 0 do i 0 io io. loop ;

0 0  io constant PA0      1 0  io constant PB0      2 0  io constant PC0
0 1  io constant PA1      1 1  io constant PB1      2 1  io constant PC1
0 2  io constant PA2      1 2  io constant PB2      2 2  io constant PC2
0 3  io constant PA3      1 3  io constant PB3      2 3  io constant PC3
0 4  io constant PA4      1 4  io constant PB4      2 4  io constant PC4
0 5  io constant PA5      1 5  io constant PB5      2 5  io constant PC5
0 6  io constant PA6      1 6  io constant PB6      2 6  io constant PC6
0 7  io constant PA7      1 7  io constant PB7      2 7  io constant PC7
0 8  io constant PA8      1 8  io constant PB8      2 8  io constant PC8
0 9  io constant PA9      1 9  io constant PB9      2 9  io constant PC9
0 10 io constant PA10     1 10 io constant PB10     2 10 io constant PC10
0 11 io constant PA11     1 11 io constant PB11     2 11 io constant PC11
0 12 io constant PA12     1 12 io constant PB12     2 12 io constant PC12
0 13 io constant PA13     1 13 io constant PB13     2 13 io constant PC13
0 14 io constant PA14     1 14 io constant PB14     2 14 io constant PC14
0 15 io constant PA15     1 15 io constant PB15     2 15 io constant PC15

3 0  io constant PD0      4 0  io constant PE0      5 0  io constant PF0
3 1  io constant PD1      4 1  io constant PE1      5 1  io constant PF1
3 2  io constant PD2      4 2  io constant PE2      5 2  io constant PF2
3 3  io constant PD3      4 3  io constant PE3      5 3  io constant PF3
3 4  io constant PD4      4 4  io constant PE4      5 4  io constant PF4
3 5  io constant PD5      4 5  io constant PE5      5 5  io constant PF5
3 6  io constant PD6      4 6  io constant PE6      5 6  io constant PF6
3 7  io constant PD7      4 7  io constant PE7      5 7  io constant PF7
3 8  io constant PD8      4 8  io constant PE8      5 8  io constant PF8
3 9  io constant PD9      4 9  io constant PE9      5 9  io constant PF9
3 10 io constant PD10     4 10 io constant PE10     5 10 io constant PF10
3 11 io constant PD11     4 11 io constant PE11     5 11 io constant PF11
3 12 io constant PD12     4 12 io constant PE12     5 12 io constant PF12
3 13 io constant PD13     4 13 io constant PE13     5 13 io constant PF13
3 14 io constant PD14     4 14 io constant PE14     5 14 io constant PF14
3 15 io constant PD15     4 15 io constant PE15     5 15 io constant PF15

$40010000 constant AFIO
     AFIO $4 + constant AFIO-MAPR

$40004800 constant USART3
   USART3 $8 + constant USART3-BRR

$40023800 constant RCC
     RCC $00 + constant RCC-CR
     RCC $04 + constant RCC-PLLCRGR
     RCC $08 + constant RCC-CFGR
\ ?  RCC $10 + constant RCC-APB1RSTR
\ ?  RCC $18 + constant RCC-APB2ENR
\ ?  RCC $1C + constant RCC-APB1ENR

$40023C00 constant FLASH
    FLASH $0 + constant FLASH-ACR

\ : -jtag ( -- )  \ disable JTAG on PB3 PB4 PA15
\   25 bit AFIO-MAPR bis! ;

\ adjusted for STM32F407 @ 168 MHz (original STM32F407 by Igor de om1zz, 2015)

25000000 variable clock-hz  \ 25 MHz crystal, HSI is 16 MHz

: 168MHz ( -- )  \ set the main clock to 168 MHz, keep baud rate at 115200
  $103 Flash-ACR !   \ 3 Flash Waitstates for 120 MHz with more than 2.7 V Vcc
                     \ Prefetch buffer enabled.
  22 bit \ PLLSRC    \ HSE clock as 25 MHz source
  25 0 lshift or  \ PLLM Division factor for main PLL and audio PLL input clock 
                  \ 25 MHz / 25 =  1 MHz. Divider before VCO. Frequency
                  \ entering VCO to be between 1 and 2 MHz.
 336 6 lshift or  \ PLLN Main PLL multiplication factor for VCO
                  \ between 192 and 432 MHz - 1 MHz * 336 = 336 MHz
  7 24 lshift or  \ PLLQ = 7, 336 MHz / 8 = 48 MHz
  0 16 lshift or  \ PLLP Division factor for main system clock
                  \ 0: /2  1: /4  2: /6  3: /8
                  \ 336 MHz / 2 = 168 MHz 
  RCC-PLLCRGR !

  24 bit RCC-CR bis!  \ PLLON - Wait for PLL to lock:
  begin 25 bit RCC-CR bit@ until  \ PLLRDY

  2                 \ Set PLL as clock source
  %101 10 lshift or \ APB  Low speed prescaler (APB1)
                    \ Max 42 MHz ! Here 168/4 MHz = 42 MHz.
  %100 13 lshift or \ APB High speed prescaler (APB2)
                    \ Max 84 MHz ! Here 168/2 MHz = 84 MHz.
  RCC-CFGR !
  168000000 clock-hz !
  $16d USART3-BRR ! \ Set Baud rate divider for 115200 Baud at 42 MHz. 22.786
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

\ : cornerstone ( "name" -- )  \ define a flash memory cornerstone
\   <builds begin here $3FF and while 0 h, repeat
\   does>   begin dup  $3FF and while 2+   repeat  cr eraseflashfrom ;
