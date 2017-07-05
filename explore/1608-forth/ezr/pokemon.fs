\ A little peek and poke monitor for the eZ80.

compiletoram? [if]  forgetram  [then]

: ?  ." PokeMon commands:"
  cr ."   z - initialise ZDI (call this one before any other command)"
  cr ."   v - display eZ80 chip version info"
  cr ."   <addr> a - set 24-bit address: upper 8 to MBASE, lower 16 to PC"
  cr ."   u - upper, shorthand for '$FFE000 a'"
  cr ."   d - dump 128 bytes of memory at current address"
  cr ."   s - show current CPU status (<run> when running)"
  cr ."   r - register dump, shows main Z80 registers"
  cr ."   b - break, stops Z80 execution"
  cr ."   c - continue, resumes Z80 execution"
  cr ."   x - hard reset, toggles the RESET pin"
  cr ."   y - soft reset, sends a ZDI reset command"
  cr ."   <byte> w - write a byte to current address and advance to next"
  cr ."   <w0> .. <w7> m - multi-word write, writes 32 bytes at once"
  cr ."   123 e - erase and unlock flash memory (the 123 arg is required)"
  cr ."   t - enter terminal mode (reset Forth to exit this mode)"
  cr ;

PB0 constant XIN
PB2 constant ZDA
PB4 constant ZCL
PB8 constant RST
PA8 constant BTN

: ez80-8MHz ( -- )
  7200 XIN pwm-init   \ first set up pwm correctly
  8 3 timer-init      \ then mess with the timer divider, i.e. ÷9
  9996 XIN pwm ;      \ finally, set the pwm to still toggle

: z ( -- )
  ZDA ios!  OMODE-OD   ZDA io-mode!
  ZCL ios!  OMODE-PP   ZCL io-mode!
  RST ios!  OMODE-OD   RST io-mode!
  BTN ios!  IMODE-PULL BTN io-mode!
  ez80-8MHz ;

: delay 10 0 do loop ;
: zcl-lo  delay ZCL ioc! delay ;
: zcl-hi  delay ZCL ios! delay ;

: zdi! ( f -- )  zcl-lo  ZDA io!  zcl-hi  ZDA ios! ;

: zdi-start ( u -- )
  ( zcl-hi ) ZDA ioc!
  OMODE-PP ZDA io-mode!
  7 0 do
    dup $40 and zdi!  shl
  loop  drop ;

: zdi> ( addr -- val )
  zdi-start  1 zdi!  1 zdi!
  OMODE-OD ZDA io-mode!
  0  8 0 do
    zcl-lo  zcl-hi
    shl  ZDA io@ 1 and or
  loop
  zcl-lo ZDA ios! zcl-hi ;

: >zdi ( val addr -- )
  zdi-start  0 zdi!  1 zdi!
  8 0 do
    dup $80 and zdi!  shl
  loop  drop
  zcl-lo ZDA ios! zcl-hi
  OMODE-OD ZDA io-mode! ;

: ins1 ( u1 -- )              $25 >zdi ;
: ins2 ( u1 u2 -- )           $24 >zdi ins1 ;
: ins3 ( u1 u2 u3 -- )        $23 >zdi ins2 ;
: ins4 ( u1 u2 u3 u4 -- )     $22 >zdi ins3 ;
: ins5 ( u1 u2 u3 u4 u5 -- )  $21 >zdi ins4 ;

: >sram ( u -- )  \ set SRAM bank
  $21 swap $80 ins3    \ ld hl,8000h+<u>
  $25 $21 $B4  ins3    \ out0 (RAM_CTL),h
  $25 $29 $B5  ins3 ;  \ out0 (RAM_BANK),l

: mb> ( -- u )            \ get MBASE
  $08 $16 >zdi            \ set adl mode
  $00 $16 >zdi  $12 zdi>  \ mbase => <u>
  $09 $16 >zdi ;          \ set z80 mode

: >mb ( u -- )  \ set MBASE
  $08 $16 >zdi            \ set adl mode
  $13 >zdi  $80 $16 >zdi  \ ld a,<u>
  $ED $6D ins2            \ ld mb,a
  $09 $16 >zdi ;          \ set z80 mode

: v  0 zdi> h.2 space  1 zdi> h.2 space  2 zdi> h.2 space ;

: s  3 zdi>
  dup 7 bit and if ." zdi " then
  dup 5 bit and if ." halt " then
  dup 4 bit and if ." adl " then
  dup 3 bit and if ." madl " then
  dup 2 bit and if ." ief1 " then
             0= if ." <run> " then ;

: b  $80 $10 >zdi ;
: c  $00 $10 >zdi ;

: r1 ( u -- )  $16 >zdi  [char] : emit  $11 zdi> h.2  $10 zdi> h.2  space ;
: r  ." FA" 0 r1  ." BC" 1 r1  ." DE" 2 r1  ." HL" 3 r1
     ." IX" 4 r1  ." IY" 5 r1  ." SP" 6 r1  ." PC" 7 r1 ;

: p ( -- u )
  $07 $16 >zdi   $11 zdi> 8 lshift  $10 zdi>  or
  mb> 16 lshift or ;

: a ( u -- )
  dup 16 rshift >mb
  dup 8 rshift $14 >zdi  $13 >zdi  $87 $16 >zdi   ;

: w ( u -- )  $30 >zdi ;

: w4 ( u -- )
  dup 24 rshift w
  dup 16 rshift w
  dup  8 rshift w
                w ;

: m ( u7..u0 -- )
  >r >r >r >r >r >r >r 
  w4 r> w4 r> w4 r> w4 r> w4 r> w4 r> w4 r> w4 ;

: d16 ( -- )
  p
  16 0 do
    $20 zdi> h.2 space
    1+ dup a
  loop
  drop ;

: d ( u -- )
  p
  8 0 do
    cr d16
  loop
  a ;

: u  $FFE000 a ;

: x  RST ioc! 1 ms RST ios! ;
: y  $80 $11 >zdi ;

: e ( 123 -- ) \ unlock and erase flash
  123 <> if cr ." try '123 e' ..." exit then
  b u
include asm/flash.fs
  u  c 500 ms b r ;

task: uart-task

: uart-reader&
  uart-task background
  begin
    begin uart-irq-key? while uart-irq-key emit repeat
    stop
  again ;

: t  \ start USART2 pass-through task
  cr uart-irq-init 19200 uart-baud
  [: uart-irq-handler uart-task wake ;] irq-usart2 !
  multitask uart-reader&
  c
  begin
    key? if key uart-emit then
    \ break out of terminal loop when button is pressed
  BTN io@ 0= until
  uart-task remove ;
