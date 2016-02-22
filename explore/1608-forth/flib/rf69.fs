\ rf69 driver
\ uses spi

       $00 constant RF:FIFO
       $01 constant RF:OP
       $07 constant RF:FRF
       $11 constant RF:PA
       $18 constant RF:LNA
       $1F constant RF:AFC
       $21 constant RF:FEI
       $24 constant RF:RSSI
       $27 constant RF:IRQ1
       $28 constant RF:IRQ2
       $2F constant RF:SYN1
       $30 constant RF:SYN2
       $39 constant RF:ADDR
       $3A constant RF:BCAST
       $3C constant RF:THRESH
       $3D constant RF:PCONF2
       $3E constant RF:AES

0 2 lshift constant RF:M_SLEEP
1 2 lshift constant RF:M_STDBY
3 2 lshift constant RF:M_TX
4 2 lshift constant RF:M_RX

       $C2 constant RF:START_TX
       $42 constant RF:STOP_TX

       $80 constant RF:RCCALSTART
1 7 lshift constant RF:IRQ1_MRDY
1 6 lshift constant RF:IRQ1_RXRDY
1 0 lshift constant RF:IRQ1_SMATCH

1 6 lshift constant RF:IRQ2_FIFO_NE
1 3 lshift constant RF:IRQ2_SENT
1 2 lshift constant RF:IRQ2_RECVD

    0 variable rf.mode
\   1 variable rf.nodeid
\ $80 variable rf.parity  \ corresponds to group 42
    0 variable rf.rssi
    0 variable rf.lna
    0 variable rf.afc
    0 variable rf.last
    66 buffer: rf.buf

create rf:init  \ initialise the radio, each 16-bit word is <reg#,val>
  $0200 h,
  $0302 h,
  $048A h,
  $0502 h,
  $06E1 h,
  $07D9 h,  \ $D92640 = 868.6 MHz
  $0826 h,
  $0940 h,
  $0B20 h,
  $194A h,
  $1A42 h,
  $1E0C h,
  $2607 h,
  $29A0 h,
  $2D05 h,
  $2E88 h,
  $2F2D h,
  $302A h,  \ group 42
  $37D0 h,
  $3842 h,
  $3C8F h,
  $3D12 h,
  $6F20 h,
  $7102 h,
      0 h,  \ marks end of init sequence
align

\ r/w access to the RF registers
: rf!@ ( b reg -- b ) +spi >spi >spi> -spi ;
: rf@ ( reg -- b ) 0 swap rf!@ ;
: rf! ( b reg -- ) $80 or rf!@ drop ;

: rf-h! ( h -- ) dup $FF and swap 8 rshift rf! ;

: rf-config! ( addr -- )
  begin  dup h@  ?dup while  rf-h!  2+ repeat drop ;

: rf!mode ( b -- )  \ set the radio mode, and store a copy in a variable
  dup rf.mode !
      RF:OP rf@  $E3 and  or RF:OP rf!
  begin  RF:IRQ1 rf@  RF:IRQ1_MRDY and  until ;

: rf-freq ( u -- )  \ set the frequency, supports any input precision
  begin dup 100000000 < while 10 * repeat
  ( f ) 2 lshift  32000000 11 rshift u/mod nip  \ avoid / use u/ instead
  ( u ) dup 10 rshift  RF:FRF rf!
  ( u ) dup 2 rshift  RF:FRF 1 + rf!
  ( u ) 6 lshift RF:FRF 2 + rf!
;

: rf-check ( b -- )  \ check that the register can be accessed over SPI
  begin  dup RF:SYN1 rf!  RF:SYN1 rf@  over = until
  drop ;

: rf-init ( freq -- )  \ TODO add nodeid and group, hard-coded for now
  spi-init
  $AA rf-check  $55 rf-check
  rf:init rf-config!
  rf-freq ;

: rf-status ( -- )  \ update status values on RXRDY
  RF:IRQ1 rf@  RF:IRQ1_RXRDY and  rf.last @ <> if
    rf.last  RF:IRQ1_RXRDY over xor!  @ if
      RF:RSSI rf@  rf.rssi !
      RF:LNA rf@  3 rshift  7 and  rf.lna !
      RF:AFC rf@  8 lshift  RF:AFC 1 + rf@  or rf.afc !
    then
  then ;

: rf-n@spi ( addr len -- )  \ send N bytes to the FIFO
  0 ?do
    RF:FIFO rf@ over c! 1+
  loop drop ;

: rf-sleep ( -- ) RF:M_SLEEP rf!mode ;  \ put radio module to sleep

: rf-recv ( -- b )  \ check whether a packet has been received, return #bytes
  rf.mode @ RF:M_RX <> if
    RF:M_RX rf!mode
  else rf-status then

  RF:IRQ2 rf@  RF:IRQ2_RECVD and if
    RF:FIFO rf@
    rf.buf over 66 max rf-n@spi
  else 0 then ;

: rfdemo ( -- )  \ display incoming packets in RF12demo format
  8686 rf-init
  cr
  begin
    rf-recv ?dup if
      ." OK "
      0 do  rf.buf i + c@ . loop
      cr
    then
  key? until ;

\ : u.2 ( u -- ) 0 <# # # #> type ;

: rfdemox ( -- )  \ display incoming packets in RF12demo HEX format
  8686 rf-init
  cr
  begin
    rf-recv ?dup if
      base @ hex swap  \ switch to hex numbers
      ." OKX "
      0 do  rf.buf i + c@ u.2  loop
      cr
      base !  \ restore previous number base
    then
  key? until ;

: rf. ( -- )  \ print out all the RF69 registers
  base @ hex
  cr 4 spaces  16 0 do space i . loop
  $60 $00 do
    cr
    i u.2 ." :"
    16 0 do  space
      i j + ?dup if rf@ u.2 else ." --" then
    loop
  $10 +loop
  base ! ;

\ 8686 rf-init rf-recv .
\ rfdemo(x)
