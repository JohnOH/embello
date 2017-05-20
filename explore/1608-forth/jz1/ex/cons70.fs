\ try out RF70 console hooks

forgetram
include ../../flib/spi/rf73.fs

     30 constant RF.OSIZE
RF.OSIZE buffer: rf.obuf
      0 variable rf.opos

: rf-flush ( -- )
  rf.opos @ if
    rf.obuf rf.opos @ 1 rf-send
    0 rf.opos !
  then ;

: rf-emit? ( -- f ) rf.opos @ RF.OSIZE < ;

: rf-emit ( c -- )
\ dup serial-emit
\ begin rf-emit? not while rf-flush repeat
\ rf-emit? not if rf-flush then
  rf.opos @ rf.obuf + c!
  1 rf.opos +! ;

: rf-emit1 ( c -- )
  rf.obuf c!
  rf.obuf 1 0 rf-send ;

: rf-poll ( -- )
  rf-flush
\ rf-recv drop
;

task: rftask
: rf& ( -- )
  rftask activate
  begin
    rf-flush
    pause
  again ;

: rfc-init
\ multitask rf&
\ ['] rf-poll  hook-pause !
  ['] rf-emit? hook-emit? !
  ['] rf-emit1 hook-emit  !
;

rf-init
\ rfc-init
