\ base definitions for STM32L053
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs io.fs

: chipid ( -- u1 u2 u3 3 )  \ unique chip ID as N values on the stack
  $1FF80050 @ $1FF80054 @ $1FF80064 @ 3 ;
: hwid ( -- u )  \ a "fairly unique" hardware ID as single 32-bit int
  chipid 1- 0 do xor loop ;
: flash-kb ( -- u )  \ return size of flash memory in KB
  $1FF8007C h@ ;
: flash-pagesize ( addr - u )  \ return size of flash page at given address
  drop 128 ;

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
\    AFIO $4 + constant AFIO-MAPR

$40013800 constant USART1
   USART1 $8 + constant USART1-BRR

$40021000 constant RCC
     RCC $00 + constant RCC-CR
     RCC $04 + constant RCC-ICSCR
     RCC $0C + constant RCC-CFGR
     RCC $28 + constant RCC-APB1RSTR
     RCC $30 + constant RCC-AHBENR
     RCC $34 + constant RCC-APB2ENR
     RCC $38 + constant RCC-APB1ENR
     RCC $4C + constant RCC-CCIPR

$40022000 constant FLASH
\   FLASH $0 + constant FLASH-ACR

16000000 variable clock-hz  \ the system clock is 16 MHz after reset

: baud ( u -- u )  \ calculate baud rate divider, based on current clock rate
  clock-hz @ swap / ;

: hsi-on
  0 bit RCC-CR bis!               \ set HSI16ON
  begin 2 bit RCC-CR bit@ until   \ wait for HSI16RDYF
;

: only-msi 8 bit RCC-CR ! ;  \ turn off HSI16, this'll disable the console UART

: 65KHz ( -- )  \ set the main clock to 65 KHz, assuming it was set to 2.1 MHz
  %111 13 lshift RCC-ICSCR bic!  65536 clock-hz ! ;

: 2.1MHz ( -- )  \ set the main clock to 2.1 MHz
  RCC-ICSCR dup @  %111 13 lshift bic  %101 13 lshift or  swap !  \ range 5
  8 bit RCC-CR bis!               \ set MSION
  begin 9 bit RCC-CR bit@ until   \ wait for MSIRDY
  %00 RCC-CFGR !                  \ revert to MSI @ 2.1 MHz, no PLL
  $101 RCC-CR !                   \ turn off HSE, and PLL
  2097000 clock-hz ! ;

: 16MHz ( -- )  \ set the main clock to 16 MHz
  hsi-on
  %01 RCC-CFGR !                  \ revert to HSI16, no PLL
  1 RCC-CR !                      \ turn off MSI, HSE, and PLL
  16000000 clock-hz ! ;

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
