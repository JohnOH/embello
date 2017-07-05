\ rf73 driver
\ uses spi

PB1 constant RF.SEL

  35 buffer:  rf.buf  \ buffer with last received packet data

create rf:init0
\ cnt  reg  data...
  1 c, $20 c,   $0F c, 
  1 c, $22 c,   $3F c, 
  1 c, $24 c,   $FF c, 
  1 c, $26 c,   $0F c,
              \ $00 c, 
              \ $38 c, 
              \ $3F c, 
  5 c, $2A c,   $4A c, $4C c, $4D c, $77 c, $01 c, 
  5 c, $2B c,   $4A c, $4C c, $4D c, $77 c, $02 c, 
  5 c, $30 c,   $4A c, $4C c, $4D c, $77 c, $01 c, 
  1 c, $3C c,   $3F c, 
  1 c, $3D c,   $07 c, 
  0 c, 
calign

create rf:init1
\ cnt   reg   data...
   4 c, $20 c,   $40 c, $4B c, $01 c, $E2 c, 
   4 c, $21 c,   $C0 c, $4B c, $00 c, $00 c, 
   4 c, $22 c,   $D0 c, $FC c, $8C c, $02 c, 
   4 c, $23 c,   $99 c, $00 c, $39 c, $41 c, 
   4 c, $24 c,   $D9 c, $96 c, $82 c, $1B c,
               \ $D9 c, $9E c, $86 c, $0B c, 
   4 c, $25 c,   $24 c, $02 c, $7F c, $A6 c, 
   4 c, $2C c,   $00 c, $12 c, $73 c, $00 c, 
   4 c, $2D c,   $46 c, $B4 c, $80 c, $00 c,
               \ $36 c, $B4 c, $80 c, $00 c, 
  11 c, $2E c,   $41 c, $20 c, $08 c, $04 c, $81 c,
                 $20 c, $CF c, $F7 c, $FE c, $FF c, $FF c, 
   4 c, $24 c,   $DF c, $96 c, $82 c, $1B c,
               \ $DF c, $9E c, $86 c, $0B c, 
   4 c, $24 c,   $D9 c, $96 c, $82 c, $1B c,
               \ $D9 c, $9E c, $86 c, $0B c, 
   0
calign

\ r/w access to the RF registers
: rf!@ ( b reg -- b ) +spi >spi >spi> -spi ;
: rf! ( b reg -- ) rf!@ drop ;
: rf@ ( reg -- b ) 0 swap rf!@ ;

: rf-sel   RF.SEL ioc! ;
: rf-desel RF.SEL ios! ;

: rx-mode ( -- ) rf-sel 0 rf@ 1 or  $20 rf! rf-desel ;
: tx-mode ( -- ) rf-sel 0 rf@ 1 bic $20 rf! rf-desel ;

: bank! ( n -- )
  7 rf@ 7 rshift xor 1 and if
    $53 $50 rf!  \ switch banks
  then ;

: c@++ ( addr -- addr b )
  dup 1+ swap c@ ;

: rf-config! ( addr -- ) \ load many registers from zero-terminated config array
  begin
    c@++
  ?dup while ( addr count )
    +spi
    >r c@++ >spi r>
    0 do c@++ >spi loop
    -spi
  repeat drop ;

: rf-init ( -- )  \ initialise the RFM73 radio module
  OMODE-PP RF.SEL io-mode!  rf-desel
  spi-init
  0 bank!
  $08 $20 rf!  \ set power down mode
  $1D rf@ 0= if
    $73 $50 rf!  \ activate extra features
  then
  rf:init0 rf-config!
  1 bank!
  rf:init1 rf-config!
  \ 8 rf@ hex.  \ will be $63 for RFM70 and RFM73
  0 bank!
  23 $25 rf!  \ use channel 23
  rx-mode ;

: rf-recv ( -- b )  \ check whether a packet has been received, return #bytes
  $17 rf@ 1 and if 0 else  \ check FIFO_STATUS_RX_EMPTY
    $60 rf@  \ get packet length w/ R_RX_PL_WID_CMD
    +spi $61 >spi
    dup 0 do
      spi> rf.buf i + c!  \ RD_RX_PLOAD
    loop
    -spi
\   +spi $E2 >spi -spi  \ FLUSH_RX
  then ;

: rf-send ( addr count hdr -- )  \ send out one packet
  if $A0 else $B0 then
  tx-mode
  +spi >spi
  0 do dup c@ >spi 1+ loop
  -spi drop
  begin
    $07 rf@ $30 and
  ?dup until
  $27 rf! rx-mode ;

: rf-info ( -- )  \ display reception parameters as hex string
  23 h.2 ;

: rf-listen ( -- )  \ init RFM73 and report incoming packets until key press
\ rf-init cr
  begin
    rf-recv ?dup if
      ." RF73 " rf-info dup h.2 space
      0 do
        rf.buf i + c@ h.2
      loop  cr
    then
  key? until ;

: rf-txtest ( n -- )  \ send out a test packet with the number as ASCII chars
  0 <# #s #> 1 rf-send ;

: rf. ( -- )  \ print out the RF73 bank 0 registers
  cr 4 spaces  base @ hex  16 0 do space i . loop  base !
  $20 $00 do
    cr
    i h.2 ." :"
    16 0 do  space
      i j + rf@ h.2
    loop
  $10 +loop ;
