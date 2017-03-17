\ Control an eZ80 microcontroller from a Blue Pill via ZDI

\ compiletoflash
\ include ../flib/stm32f1/uart2.fs

compiletoram? [if]  forgetram  [then]

PB0 constant XIN
PB1 constant RST
PB4 constant ZCL
PB5 constant ZDA

: ez80-8MHz ( -- )
  7200 XIN pwm-init   \ first set up pwm correctly
  8 3 timer-init     \ then mess with the timer divider, i.e. ÷9
  9998 XIN pwm ;      \ finally, set the pwm to still toggle

: zdi-init ( -- )
  RST ios!  OMODE-OD RST io-mode!
  ZDA ios!  OMODE-OD ZDA io-mode!
  ZCL ios!  OMODE-PP ZCL io-mode!
  ez80-8MHz ;

: delay 5 0 do loop ;
: zcl-lo  delay ZCL ioc!  delay ;
: zcl-hi  delay ZCL ios!  delay ;

: zdi! ( f -- )  zcl-lo  ZDA io!  zcl-hi  ZDA ios! ;

: zdi-start ( u -- )
  ( zcl-hi ) ZDA ioc!
  7 0 do
    dup $40 and zdi!  shl
  loop  drop ;

: zdi> ( addr -- val )
  zdi-start  1 zdi!  1 zdi!
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
  zcl-lo ZDA ios! zcl-hi ;

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

: p ( -- u )  $07 $16 >zdi   $11 zdi> 8 lshift  $10 zdi>  or ;
: a ( u -- )  dup 8 rshift $14 >zdi  $13 >zdi  $87 $16 >zdi   ;

: m
  p  16 0 do
    $20 zdi> h.2 space
    1+ dup a
  loop
  16 - a ;

: w ( u -- )  $30 >zdi ;
: f ( addr size -- )  0 do dup c@ w 1+ loop drop ;

                512 buffer: page
page $FF + $FF bic constant sect  \ 256-byte aligned for cleaner dump output

: ins1 ( u1 -- )              $25 >zdi ;
: ins2 ( u1 u2 -- )           $24 >zdi ins1 ;
: ins3 ( u1 u2 u3 -- )        $23 >zdi ins2 ;
: ins4 ( u1 u2 u3 u4 -- )     $22 >zdi ins3 ;
: ins5 ( u1 u2 u3 u4 u5 -- )  $21 >zdi ins4 ;

: sram> ( -- u )  \ get SRAM bank
  $08 $16 >zdi            \ set adl mode
  $00 $16 >zdi  $12 zdi>  \ mbase => <u>
  $09 $16 >zdi ;          \ set z80 mode

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

: d ( u -- )
  dup 16 rshift >mb  a
  8 0 do
    cr m
    p 16 + a
  loop
  p 128 - a ;

: u  $FF >mb  $E000 a ;

: s1 ( -- )  \ show output from USART2
  ." Hit return to exit serial mode:" cr
  uart-init  19200 baud 2/ USART2-BRR !
  c
  begin
    uart-key? if uart-key emit then
  key? until b r ;

: x  RST ioc! 1 ms RST ios! ;

: h  \ send greeting over serial, see asm/hello.asm
  b u
include asm/hello.fs
  u s1 ;

: l  \ send greeting over serial, see asm/hellow.asm, in low mem
  b u $0100 a
include asm/hellow.fs
  u $0100 a s1 ;

: q ( n -- ) \ perform step N of flash setup (n ≥ 0)
  b u
include asm/flash.fs
  u  3 * $E000 + a  c 500 ms b r ;

: z $3A6000 d s1 ;

: ?
  cr ." v = show chip version           b = break next "
  cr ." s = show processor state        c = continue "
  cr ." r = show registers              u = upper pc ($FFE000) "
  cr ." m = show memory                 p = get PC ( -- u ) "
  cr ." w = write memory ( b -- )       a = set PC address ( u -- ) "
  cr ." f = fill memory ( addr n -- )   h = high serial test ($E000) "
  cr ." d = disk dump ( u -- )          l = low serial test ($0080) "
  cr ." x = hardware reset              q = flash request ( u --) "
  cr ." z = start running at $3A6000    ? = this help "
  cr ;

: s2
  cr uart-init  19200 baud 2/ USART2-BRR !
  c
  begin
    uart-key? if uart-key emit then
    key? if key uart-emit then
  again ;

: go  $3A >mb $6000 a s2 ;

: w4 ( u -- )
  dup 24 rshift w
  dup 16 rshift w
  dup  8 rshift w
                w ;
: w32 ( u7..u0 -- )
  >r >r >r >r >r >r >r 
  w4 r> w4 r> w4 r> w4 r> w4 r> w4 r> w4 r> w4 ;

zdi-init  cr x b v s cr r
