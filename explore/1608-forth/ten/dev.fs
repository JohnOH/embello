\ main code

compiletoram? [if]  forgetram  [then]

: rfmods
  $E4 $29 rf!
  $0F $2D rf!
  $A0 $2E rf!
  $55 $2F rf!
  $2D $30 rf!
  $2A $31 rf!
  $2D $58 rf!
  rf. cr ;

: blip 
  870 rf.freq !  rf-init  8 rf-power
  rfmods
  1000000 0 do
    5 <pkt  i +pkt  chipid 0 do +pkt loop  pkt>rf
    5000 0 do
      rf-recv ?dup if
        cr j . rf.buf 2+ swap 2- var.
      then
    loop
    1000 ms
  loop ;

: u>s16 ( u -- n ) 16 lshift 16 arshift ;
: u>s8 ( u -- n ) 24 lshift 24 arshift ;

: echo
  870 rf.freq !  rf-init  0 rf-power
  rfmods
  begin
    rf-recv ?dup if
      cr rf.buf 2+ swap 2- var.
      7 <pkt  rf.rssi @       dup . +pkt
               rf.lna @       dup . +pkt
               rf.afc @ u>s16 dup . +pkt  pkt>rf
    then
    [ifdef] usb-poll  usb-poll  [then]
  again ;

\ echo
