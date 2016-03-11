\ base definitions for STM32F103
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs io.fs

: chipid ( -- u1 u2 u3 3 )  \ unique chip ID as N values on the stack
  $1FFFF7E8 @ $1FFFF7EC @ $1FFFF7F0 @ 3 ;
: hwid ( -- u )  \ a "fairly unique" hardware ID as single 32-bit int
  chipid 1- 0 do xor loop ;
: flash-kb ( -- u )  \ return size of flash memory in KB
  $1FFFF7E0 h@ ;
: flash-pagesize ( addr - u )  \ return size of flash page at given address
  drop flash-kb 128 <= if 1024 else 2048 then ;

: io.all ( -- )  \ display all the readable GPIO registers
  io-ports 0 do i 0 io io. loop ;

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
     RCC $14 + constant RCC-AHBENR
     RCC $18 + constant RCC-APB2ENR
     RCC $1C + constant RCC-APB1ENR

$40022000 constant FLASH
    FLASH $0 + constant FLASH-ACR

: -jtag ( -- )  \ disable JTAG on PB3 PB4 PA15
  25 bit AFIO-MAPR bis! ;

\ adjusted for STM32F103 @ 72 MHz (original STM32F100 by Igor de om1zz, 2015)

8000000 variable clock-hz  \ the system clock is 8 MHz after reset

: baud ( u -- u )  \ calculate baud rate divider, based on current clock rate
  clock-hz @ swap / ;

: 72MHz ( -- )  \ set the main clock to 72 MHz, keep baud rate at 115200
  2 FLASH-ACR bis!                \ two flash mem wait states
  16 bit RCC-CR bis!              \ set HSEON
  begin 17 bit RCC-CR bit@ until  \ wait for HSERDY
  1 16 lshift                     \ HSE clock is 8 MHz Xtal source for PLL
  7 18 lshift or                  \ PLL factor: 8 MHz * 9 = 72 MHz = HCLK
  4  8 lshift or                  \ PCLK1 = HCLK/2
  2 14 lshift or                  \ ADCPRE = PCLK2/6
            2 or  RCC-CFGR !      \ PLL is the system clock
  24 bit RCC-CR bis!              \ set PLLON
  begin 25 bit RCC-CR bit@ until  \ wait for PLLRDY
  72000000 clock-hz !  115200 baud USART1-BRR !  \ fix console baud rate
;

0 variable ticks

: ++ticks ( -- ) 1 ticks +! ;  \ for use as systick irq handler

: systick-hz ( u -- )  \ enable systick interrupt at given frequency
  ['] ++ticks irq-systick !
  clock-hz @ swap /  1- $E000E014 !  7 $E000E010 ! ;

: micros ( -- n )  \ return elapsed microseconds, this wraps after some 2000s
\ assumes systick is running at 1000 Hz, overhead is about 1.8 us @ 72 MHz
\ get current ticks and systick, spinloops if ticks changed while we looked
  0 dup  begin 2drop  ticks @ $E000E018 @  over ticks @ = until
  $E000E014 @ 1+ swap -  \ convert down-counter to remaining
  clock-hz @ 1000000 / ( ticks systicks mhz )
  / swap 1000 * + ;

: millis ( -- u )  \ return elapsed milliseconds, this wraps after 49 days
  ticks @ ;

: us ( n -- )  \ microsecond delay using a busy loop, this won't switch tasks
  3 -  \ adjust for approximate overhead of this code itself
  micros +  begin dup micros - 0< until  drop ;

: ms ( n -- )  \ millisecond delay, current limit is about 2000s
  1000 * us ;  \ TODO need to change this to support multitasking

\ : j0 micros 1000000 0 do 1 us loop micros swap - . ;
\ : j1 micros 1000000 0 do 5 us loop micros swap - . ;
\ : j2 micros 1000000 0 do 10 us loop micros swap - . ;
\ : j3 micros 1000000 0 do 20 us loop micros swap - . ;
\ : jn j0 j1 j2 j3 ;  \ sample results: 4065044 5988036 10542166 20833317

\ emulate c, which is not available in hardware on some chips.
\ copied from Mecrisp's common/charcomma.txt
0 variable c,collection

: c, ( c -- )  \ emulate c, with h,
  c,collection @ ?dup if $FF and swap 8 lshift or h,
                         0 c,collection !
                      else $100 or c,collection ! then ;

: calign ( -- )  \ must be called to flush after odd number of c, calls
  c,collection @ if 0 c, then ;

: list ( -- )  \ list all words in dictionary, short form
  cr dictionarystart begin
    dup 6 + ctype space
  dictionarynext until drop ;

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  <builds begin here dup flash-pagesize 1- and while 0 h, repeat
  does>   begin dup  dup flash-pagesize 1- and while 2+   repeat  cr eraseflashfrom ;
