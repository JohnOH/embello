\ Control an eZ80 microcontroller from a Blue Pill via ZDI

singletask

\ compiletoflash
\ include ../flib/stm32f1/spi2.fs
\ include ../flib/stm32f1/uart2.fs

compiletoram? [if]  forgetram  [then]

include ex/sdtry.fs

PC13 constant LED
PA8 constant BUSY

PB0 constant XIN
PB1 constant RST
PB4 constant ZCL
PB5 constant ZDA

$E000E100 constant NVIC-EN0R \ IRQ 0 to 31 Set Enable Register

AFIO $08 + constant AFIO-EXTICR1
AFIO $0C + constant AFIO-EXTICR2
AFIO $10 + constant AFIO-EXTICR3
AFIO $14 + constant AFIO-EXTICR4

$40010400 constant EXTI
    EXTI $00 + constant EXTI-IMR
    EXTI $08 + constant EXTI-RTSR
    EXTI $0C + constant EXTI-FTSR
    EXTI $14 + constant EXTI-PR

\ $40020000 constant DMA1
\   DMA1 $00 + constant DMA1-ISR
\   DMA1 $04 + constant DMA1-IFCR
    DMA1 $44 + constant DMA1-CCR4
    DMA1 $48 + constant DMA1-CNDTR4
    DMA1 $4C + constant DMA1-CPAR4
    DMA1 $50 + constant DMA1-CMAR4
    DMA1 $58 + constant DMA1-CCR5
    DMA1 $5C + constant DMA1-CNDTR5
    DMA1 $60 + constant DMA1-CPAR5
    DMA1 $64 + constant DMA1-CMAR5

5 buffer: xyz  \ alignment

\ 512 buffer: frdbuf
520 buffer: frdbuf
516 buffer: fwrbuf

: led-setup  LED ioc!  OMODE-PP LED io-mode! ;

: dma-setup
  0 bit RCC-AHBENR bic!  \ DMA1EN clock disable
  0 bit RCC-AHBENR bis!  \ DMA1EN clock enable

  \ DMA1 channel 4: from SPI2 RX to fwrbuf, 1..516 bytes
   fwrbuf DMA1-CMAR4 !     \ write to fw-buffer
  SPI2-DR DMA1-CPAR4 !     \ read from SPI2
     516 DMA1-CNDTR4 !
                0   \ register settings for CCR4 of DMA1:
          7 bit or  \ MINC
                    \ DIR = from peripheral to mem
          0 bit or
      DMA1-CCR4 !

  \ DMA1 channel 5: from frdbuf to SPI2 TX, 1..513 bytes
   frdbuf DMA1-CMAR5 !     \ read from fr-buffer
  SPI2-DR DMA1-CPAR5 !     \ write to SPI2
     516 DMA1-CNDTR5 !
                0   \ register settings for CCR5 of DMA1:
          7 bit or  \ MINC
          4 bit or  \ DIR = from mem to peripheral
          0 bit or
      DMA1-CCR5 !
;

: spi2-setup
  IMODE-PULL ssel2 @ io-mode! -spi2
  IMODE-FLOAT SCLK2 io-mode!
  OMODE-AF-PP MISO2 io-mode!
  IMODE-PULL MOSI2 io-mode!  MOSI2 ioc!
  14 bit RCC-APB1ENR bic!  \ clear SPI2EN
  14 bit RCC-APB1ENR bis!  \ set SPI2EN
  %11 SPI2-CR2 !  \ enable TX and RX DMA
  6 bit SPI2-CR1 !  \ slave mode, enable
\ $FF SPI2-DR !  \ prime the SPI status reply
;

task: disktask

: disk&
  disktask background
  begin
    LED iox!
\   cr ." <!>" fwrbuf @ hex. DMA1-CNDTR4 @ . \ SPI2-SR @ hex.
    0 DMA1-CCR5 !
    0 DMA1-CCR4 !
    SPI2-DR @ drop SPI2-SR @ drop  \ clear SPI2 buffers and errors
    0 SPI2-CR1 !  \ disable
    fwrbuf c@ 3 = if
      fwrbuf @ 8 rshift
      dup 9 rshift sd-read
      $180 and sd.buf + frdbuf 128 move
    then
    dma-setup
    spi2-setup
    BUSY ioc!
    LED iox!
    stop
  again ;

: firq ( -- )  BUSY ios!  12 bit EXTI-PR !  disktask wake ;

: firq-setup  \ set up pin interrupt on rising spi2 slave select on PB12
  OMODE-PP BUSY io-mode!  BUSY ioc!

  ['] firq irq-exti10 !

     8 bit NVIC-EN1R bis!  \ enable EXTI15_10 interrupt 40
  %0001 AFIO-EXTICR4 bis!  \ select P<B>12
     12 bit EXTI-IMR bis!  \ enable PB<12>
    12 bit EXTI-RTSR bis!  \ trigger on PB<12> rising edge
;

: ez80-8MHz ( -- )
  7200 XIN pwm-init   \ first set up pwm correctly
  8 3 timer-init      \ then mess with the timer divider, i.e. ÷9
  9996 XIN pwm ;      \ finally, set the pwm to still toggle

: zdi-init ( -- )
  RST ios!  OMODE-OD RST io-mode!
  ZDA ios!  OMODE-OD ZDA io-mode!
  ZCL ios!  OMODE-PP ZCL io-mode!
  ez80-8MHz ;

: init-all
  sd-init sd-size .
  multitask disk&
  zdi-init led-setup
  firq-setup dma-setup spi2-setup ;

: delay 100 0 do loop ;
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

: s1  \ show output from USART2
  uart-init 19200 baud 2/ USART2-BRR !
  c
  1000000 0 do
    uart-key? if uart-key emit then
  loop cr b r ;

: s2  \ switch to permanent USART2 pass-through
  cr uart-init 19200 baud 2/ USART2-BRR !
  c
  begin
    uart-key? if uart-key emit then
    \ break out of terminal loop when ctrl-x is seen
    key? if key dup 24 = if drop exit then uart-emit then
\   pause
  again ;

: x  RST ioc! 1 ms RST ios! ;

: h  \ send greeting over serial, see asm/hello.asm
  b u
include asm/hello.fs
  u s1 ;

: l  \ send greeting over serial, see asm/hellow.asm, in low mem
  b u $0100 a
include asm/hellow.fs
  u $0100 a s1 ;

: euf ( -- ) \ unlock and erase flash
  b u
include asm/flash.fs
  u  $E000 a  c 500 ms b r ;

\ : z $3A6000 d s1 ;
: z init-all s2 ;

: ?
  cr ." v = show chip version           b = break next "
  cr ." s = show processor state        c = continue "
  cr ." r = show registers              u = upper pc ($FFE000) "
  cr ." m = show memory                 p = get PC ( -- u ) "
  cr ." w = write memory ( b -- )       a = set PC address ( u -- ) "
  cr ." f = fill memory ( addr n -- )   h = high serial test ($E000) "
  cr ." d = disk dump ( u -- )          l = low serial test ($0080) "
  cr ." x = hardware reset              euf = erase and unlock flash "
  cr ." z = start zdi and serial        ? = this help "
  cr ;

: go  $3A >mb $6000 a s2 ;

: w4 ( u -- )
  dup 24 rshift w
  dup 16 rshift w
  dup  8 rshift w
                w ;
: w32 ( u7..u0 -- )
  >r >r >r >r >r >r >r 
  w4 r> w4 r> w4 r> w4 r> w4 r> w4 r> w4 r> w4 ;

\ init-all  cr x b v s cr r
