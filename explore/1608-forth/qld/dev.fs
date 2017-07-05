\ development code

forgetram

PA0 constant RESET-PIN
PA1 constant BOOT0-PIN

0 variable xsum

: boot-init
  spi2-init  +spi2  \ permanently held low
\ %0000000001010100 SPI2-CR1 !  \ clk/8, i.e. 4.5 MHz, master
  uart-irq-init
  RESET-PIN ioc!  OMODE-PP RESET-PIN io-mode!
  BOOT0-PIN ios!  OMODE-PP BOOT0-PIN io-mode!
;

: boot-mode
  RESET-PIN ioc!
  BOOT0-PIN ios!
  2 ms
  RESET-PIN ios!
  5 ms ;

0 [if]
: >spi2>  >spi2> [char] < emit dup h.2 ;
: >spi2   >spi2> drop ;
: spi2>   0 >spi2> ;
[then]

: wait-ack ( -- f )
  $00 >spi2
  begin
    $00 >spi2>
  dup $79 <> over $1F <> and while
    $A5 = if $5A >spi2 then
    100 us
  repeat
  $79 >spi2
  $79 = ;

: check-ack ( -- )
  wait-ack not if ."  NAK?" usb-poll reset then ;

: send ( b -- )  dup >spi2  xsum @ xor xsum ! ;
: send2 ( n -- )  dup 8 rshift send  send ;
: send4 ( u -- )  dup 16 rshift send2  send2 ;
: sof ( -- )  $5A >spi2  $FF xsum ! ;
: cmd ( b -- )  sof send  xsum @ send  check-ack ;

: get-cmd ( -- n )
  $00 cmd  $00 >spi2  $00 >spi2>  $00 >spi2>
  swap 0 do $00 >spi2 loop check-ack ;
: get-version ( -- n )
  $01 cmd  $00 >spi2  $00 >spi2>  $00 >spi2 check-ack ;
: get-id ( -- n )
  $02 cmd  $00 >spi2  $00 >spi2> 1+
  0 swap 0 do 8 lshift spi2> or loop check-ack ;
: rd-unp ( -- )
  $92 cmd  check-ack check-ack ;
: wr-unp ( -- )
  $73 cmd  check-ack check-ack ;
: erase ( n -- )
  $44 cmd  dup 1- send2  xsum @ send check-ack
           0 do i send2 loop  xsum @ send check-ack ;
: pgm ( a u -- )
  $31 cmd  send4  xsum @ send check-ack
  127 send
  128 0 do dup c@ send 1+ loop  xsum @ send check-ack  drop ;

: uploader ( -- f )
  millis
  boot-mode
  sof check-ack
  get-cmd hex . decimal
\ get-version hex . decimal
  get-id hex . decimal
  rd-unp
  wr-unp
  key? drop  \ forces flushing if using USB
  512 erase
  320 0 do ( [char] + emit ) 0 i 128 * $08000000 + pgm loop
  cr millis swap - . ." ms "
;

boot-init
\ uploader
