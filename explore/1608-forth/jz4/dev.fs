\ send out periodic weak pings on a non-standard frequency

compiletoram? [if]  forgetram  [then]

: rfmods
  $E4 $29 rf!
  $0F $2D rf!
  $A0 $2E rf!
  $55 $2F rf!
  $2D $30 rf!
  $2A $31 rf!
  $2D $58 rf!
  ( rf. ) ;

: disable-console
  ['] false hook-key?  !
  ['] bl    hook-key   !
  ['] true  hook-emit? !
  ['] drop  hook-emit  ! ;

: show-reply rf.buf 2+ swap 2- var. ;

: blip 
  870 rf.freq !  rf-init  8 rf-power
  rfmods
  5 0 do
    5 <pkt  i +pkt  chipid 0 do +pkt loop  pkt>rf
    5000 0 do
      rf-recv ?dup if
        cr j . show-reply cr cr
        disable-console  <<<core>>>  \ clears test code and does a s/w reset
      then
    loop
    1000 ms
  loop ;

\ blip
