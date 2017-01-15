\ explore RF69 driver on Tiny Extender

: rf-listenv ( -- )  \ init RFM69 and report incoming packets until key press
  rf-init cr
  begin
    rf-recv ?dup if
      ." RF69 " rf-info
      ( len ) dup 0 do
        rf.buf i + c@ h.2
        i 1 = if dup 2- h.2 space then
      loop  cr
      ( len ) 5 spaces rf.buf 2+ swap 2- var. cr  \ decode and show varints
    then
  key? until ;

\ this is a variant of rf-listen which also shows the received varints
6 rf.group !
1234 ms rf-listenv
