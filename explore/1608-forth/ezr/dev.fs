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
  17 3 timer-init      \ then mess with the timer divider, i.e. ÷18
  9998 XIN pwm        \ finally, set the pwm to still toggle
  RST ios!  OMODE-OD RST io-mode! ;

: ez80-reset ( -- )  RST ioc! 1 ms RST ios! ;

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

: pc> ( -- u )  $07 $16 >zdi   $11 zdi> 8 lshift  $10 zdi>  or ;
: >pc ( u -- )  dup 8 rshift $14 >zdi  $13 >zdi  $87 $16 >zdi   ;

: m
  pc>  8 0 do
    $20 zdi> h.2 space
    1+ dup >pc
  loop
  8 - >pc ;

: w ( u -- )  $30 >zdi ;

                512 buffer: page
page $FF + $FF bic constant sect  \ 256-byte aligned for cleaner dump output

: d ( u -- )
  drop \ TODO get 128 bytes into sect
  sect 128 dump ;

: x  $08 $16 >zdi  $FF $13 >zdi  $80 $16 >zdi  
     $6D $24 >zdi  $ED $25 >zdi  $09 $16 >zdi  $E000 >pc ;

: ?
  cr ." v = show chip version         b = break next "
  cr ." s = show processor state      c = continue "
  cr ." r = show registers "
  cr ." m = read memory               pc> = get PC ( -- u ) "
  cr ." w = write memory ( b -- )     >pc = set PC ( u -- ) "
  cr ." d = disk read ( u -- )        x = reset pc bank "
  cr ." ? = this help "
  cr ;

: ez80-hello  \ send greeting over serial, see asm/hello.asm
  b x
include asm/hello.fs
  x c ;

: ez80-hellow  \ send greeting over serial, see asm/hellow.asm, in low mem
  b x $0080 >pc
include asm/hellow.fs
  x $0080 >pc c ;

: serial-pass ( -- )  \ pass all I/O to/from USART2
  uart-init  9600 baud 2/ USART2-BRR !
  begin
    uart-key? if uart-key emit then
  key? until ;

: serial-test ez80-hello serial-pass ;
: serial-testw ez80-hellow serial-pass ;

: ez80-flash ( n -- ) \ perform step N of flash setup (n ≥ 0)
  b x
include asm/flash.fs
  x  3 * $E080 + >pc  c ;

ez80-4MHz  zdi-init  100 ms  cr ? cr v b s cr r
