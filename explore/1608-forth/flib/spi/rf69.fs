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

     7 bit constant RF:IRQ1_MRDY
     6 bit constant RF:IRQ1_RXRDY
     0 bit constant RF:IRQ1_SMATCH

     6 bit constant RF:IRQ2_FIFO_NE
     3 bit constant RF:IRQ2_SENT
     2 bit constant RF:IRQ2_RECVD

   0 variable rf.mode
   0 variable rf.rssi
   0 variable rf.lna
   0 variable rf.afc
   0 variable rf.last

   66 buffer: rf.buf

   1 variable rf69.nodeid
  42 variable rf69.group
8686 variable rf69.freq

create rf:init  \ initialise the radio, each 16-bit word is <reg#,val>
hex
  0200 h, 0302 h, 048A h, 0502 h, 06E1 h, 0B20 h, 194A h, 1A42 h,
  1E0C h, 2607 h, 29A0 h, 2D05 h, 2E88 h, 2F2D h, 302A h, 37D0 h,
  3842 h, 3C8F h, 3D12 h, 6F20 h, 7102 h, 0 h,  \ sentinel
decimal align

\ r/w access to the RF registers
: rf!@ ( b reg -- b ) +spi >spi >spi> -spi ;
: rf! ( b reg -- ) $80 or rf!@ drop ;
: rf@ ( reg -- b ) 0 swap rf!@ ;

: rf-h! ( h -- ) dup $FF and swap 8 rshift rf! ;
: rf-config! ( addr -- ) begin  dup h@  ?dup while  rf-h!  2+ repeat drop ;

: rf!mode ( b -- )  \ set the radio mode, and store a copy in a variable
  dup rf.mode !
  RF:OP rf@  $E3 and  or RF:OP rf!
  begin  RF:IRQ1 rf@  RF:IRQ1_MRDY and  until ;

: rf-freq ( u -- )  \ set the frequency, supports any input precision
  begin dup 100000000 < while 10 * repeat
  ( f ) 2 lshift  32000000 11 rshift u/mod nip  \ avoid / use u/ instead
  ( u ) dup 10 rshift  RF:FRF rf!
  ( u ) dup 2 rshift  RF:FRF 1+ rf!
  ( u ) 6 lshift RF:FRF 2+ rf!
;

: rf-group ( u -- ) RF:SYN2 rf! ;  \ set the net group (1..250)

: rf-power ( n -- )  \ change TX power level (0..31)
  RF:PA rf@ $E0 and or RF:PA rf! ;

: rf-check ( b -- )  \ check that the register can be accessed over SPI
  begin  dup RF:SYN1 rf!  RF:SYN1 rf@  over = until
  drop ;

: rf-init ( group freq -- )  \ init the RFM69 radio module
  spi-init
  $AA rf-check  $55 rf-check  \ will hang if there is no radio!
  rf:init rf-config!
  rf-freq rf-group ;

: rf-status ( -- )  \ update status values on RXRDY
  RF:IRQ1 rf@  RF:IRQ1_RXRDY and  rf.last @ <> if
    rf.last  RF:IRQ1_RXRDY over xor!  @ if
      RF:RSSI rf@  rf.rssi !
      RF:LNA rf@  3 rshift  7 and  rf.lna !
      RF:AFC rf@  8 lshift  RF:AFC 1+ rf@  or rf.afc !
    then
  then ;

: rf-n@spi ( addr len -- )  \ read N bytes from the FIFO
  0 do  RF:FIFO rf@ over c! 1+  loop drop ;
: rf-n!spi ( addr len -- )  \ write N bytes to the FIFO
  0 do  dup c@ RF:FIFO rf! 1+  loop drop ;

: rf-sleep ( -- ) RF:M_SLEEP rf!mode ;  \ put radio module to sleep

: rf-recv ( -- b )  \ check whether a packet has been received, return #bytes
  rf.mode @ RF:M_RX <> if
    RF:M_RX rf!mode
  else rf-status then
  RF:IRQ2 rf@  RF:IRQ2_RECVD and if
    RF:FIFO rf@
    rf.buf over 66 max rf-n@spi
  else 0 then ;

: rf-parity ( -- u )  \ calculate group parity bits
  RF:SYN2 rf@ dup 4 lshift xor dup 2 lshift xor $C0 and ;

: rf-send ( addr count hdr -- )  \ send out one packet
  RF:M_STDBY rf!mode
  over 2+ RF:FIFO rf!
  dup rf-parity or RF:FIFO rf!
  $C0 and rf69.nodeid @ or RF:FIFO rf!
  ( addr count ) rf-n!spi
  RF:M_TX rf!mode
  begin RF:IRQ2 rf@ RF:IRQ2_SENT and until
  RF:M_STDBY rf!mode ;

\ new code starts here, this is the intended public API for the RF69 driver

: rf69-init ( -- )  \ init RFM69 with current rf69.group and rf69.freq values
  rf69.group @ rf69.freq @ rf-init ;

: rf69-info ( -- )  \ display reception parameters as hex string
  rf69.freq @ h.4 rf69.group @ h.2 rf.rssi @ h.2 rf.lna @ h.2 rf.afc @ h.4 ;

: rf69-listen ( -- )  \ init RFM69 and report incoming packets until key press
  rf69-init cr
  begin
    rf-recv ?dup if
      ." RF69 " rf69-info
      dup 0 do
        rf.buf i + c@ h.2
        i 1 = if 2- h.2 space then
      loop  cr
    then
  key? until ;

: rf69. ( -- )  \ print out all the RF69 registers
  cr 4 spaces  base @ hex  16 0 do space i . loop  base !
  $60 $00 do
    cr
    i h.2 ." :"
    16 0 do  space
      i j + ?dup if rf@ h.2 else ." --" then
    loop
  $10 +loop ;

: rf69-txtest ( n -- )  \ send out a test packet with the number as ASCII chars
  rf69-init  16 rf-power  0 <# #s #> 0 rf-send ;

\ rf69.
\ rf69-listen
\ 12345 rf69-txtest
