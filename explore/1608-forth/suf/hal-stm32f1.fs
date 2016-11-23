\ base definitions for STM32F103 (STRIPPED-DOWN version for USB driver use!)
\ adapted from mecrisp-stellaris 2.2.1a (GPL3)
\ needs the definition of "bit" from io.fs

: chipid ( -- u1 u2 u3 3 )  \ unique chip ID as N values on the stack
  $1FFFF7E8 @ $1FFFF7EC @ $1FFFF7F0 @ 3 ;
: hwid ( -- u )  \ a "fairly unique" hardware ID as single 32-bit int
  chipid 1 do xor loop ;

$40010000 constant AFIO
     AFIO $4 + constant AFIO-MAPR

$40013800 constant USART1
   USART1 $8 + constant USART1-BRR

$40021000 constant RCC
     RCC $00 + constant RCC-CR
     RCC $04 + constant RCC-CFGR
     RCC $1C + constant RCC-APB1ENR

$40022000 constant FLASH
    FLASH $0 + constant FLASH-ACR

\ adjusted for STM32F103 @ 72 MHz (original STM32F100 by Igor de om1zz, 2015)

: 72MHz ( -- )  \ set the main clock to 72 MHz, keep baud rate at 115200
  $12 FLASH-ACR !                 \ two flash mem wait states
  16 bit RCC-CR bis!              \ set HSEON
  begin 17 bit RCC-CR bit@ until  \ wait for HSERDY
  1 16 lshift                     \ HSE clock is 8 MHz Xtal source for PLL
  7 18 lshift or                  \ PLL factor: 8 MHz * 9 = 72 MHz = HCLK
  4  8 lshift or                  \ PCLK1 = HCLK/2
  2 14 lshift or                  \ ADCPRE = PCLK2/6
            2 or  RCC-CFGR !      \ PLL is the system clock
  24 bit RCC-CR bis!              \ set PLLON
  begin 25 bit RCC-CR bit@ until  \ wait for PLLRDY
  625 USART1-BRR !                \ fix console baud rate
;

\ emulate c, which is not available in hardware on some chips.
\ copied from Mecrisp's common/charcomma.txt
0 variable c,collection

: c, ( c -- )  \ emulate c, with h,
  c,collection @ ?dup if $FF and swap 8 lshift or h,
                         0 c,collection !
                      else $100 or c,collection ! then ;

: calign ( -- )  \ must be called to flush after odd number of c, calls
  c,collection @ if 0 c, then ;

: cornerstone ( "name" -- )  \ define a flash memory cornerstone
  \ round to 2K pages, even when generated on a chip which supports 1K
  <builds begin here $7FF and while 0 h, repeat
  does>   begin dup  $7FF and while 2+   repeat  cr
  eraseflashfrom ;
