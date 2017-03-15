\ Control an eZ80 microcontroller from a Blue Pill via ZDI

\ compiletoflash
\ include ../flib/stm32f1/uart2.fs

compiletoram? [if]  forgetram  [then]

PB0 constant XIN
PB1 constant RST
PB4 constant ZCL
PB5 constant ZDA

: ez80-4MHz ( -- )
  7200 XIN pwm-init   \ first set up pwm correctly
  17 3 timer-init     \ then mess with the timer divider, i.e. ÷18
  9998 XIN pwm        \ finally, set the pwm to still toggle
  RST ios!  OMODE-OD RST io-mode! ;

: zdi-init ( -- )
  ZDA ios!  OMODE-OD ZDA io-mode!
  ZCL ios!  OMODE-PP ZCL io-mode! ;

: zcl-lo  10 us ZCL ioc!  10 us ;
: zcl-hi  10 us ZCL ios!  10 us ;

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

: b  7 bit $10 >zdi ;

: c  0 $10 >zdi ;

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

: f ( addr u -- )  0 do dup c@ w 1+ loop drop ;

                512 buffer: page
page $FF + $FF bic constant sect  \ 256-byte aligned for cleaner dump output

: d ( u -- )
  drop \ TODO get 128 bytes into sect
  sect 128 dump ;

: u  $08 $16 >zdi  $FF $13 >zdi  $80 $16 >zdi  
     $6D $24 >zdi  $ED $25 >zdi  $09 $16 >zdi  $E000 a ;

: serial-show ( -- )  \ show output from USART2
  uart-init  9600 baud 2/ USART2-BRR !
  begin
    uart-key? if uart-key emit then
  key? until ;

: x  RST ioc! 1 ms RST ios! ;

: h  \ send greeting over serial, see asm/hello.asm
  b u
include asm/hello.fs
  u c serial-show ;

: l  \ send greeting over serial, see asm/hellow.asm, in low mem
  b u $0080 a
include asm/hellow.fs
  u $0080 a c serial-show ;

: q ( n -- ) \ perform step N of flash setup (n ≥ 0)
  b u
include asm/flash.fs
  u  3 * $E000 + a  c ;

: ?
  cr ." v = show chip version           b = break next "
  cr ." s = show processor state        c = continue "
  cr ." r = show registers              u = upper pc ($FFE000) "
  cr ." m = show memory                 p = get PC ( -- u ) "
  cr ." w = write memory ( b -- )       a = set PC address ( u -- ) "
  cr ." f = fill memory ( addr n -- )   h = high serial test ($E000) "
  cr ." d = disk dump ( u -- )          l = low serial test ($0080) "
  cr ." x = hardware reset              q = flash request ( u --) "
  cr ." ? = this help "
  cr ;

ez80-4MHz  zdi-init  100 ms  cr ? cr v b s cr r
\ include cpm/disk.fs
