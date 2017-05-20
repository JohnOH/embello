\ RF70 console pass-through

forgetram
include ../../flib/spi/rf73.fs

: rf-pass ( -- )
  begin
    rf-recv 0 ?do
      rf.buf i + c@ emit
    loop
\   key? if
\   then
\ again ;
  key? until ;

rf-init
1234 ms rf-pass
