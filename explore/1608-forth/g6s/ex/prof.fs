\ try out the profile utility

forgetram

: a ;
: b 0 do a loop ;
: c dup 0 do b loop ;

include ../../flib/mecrisp/disassembler-m3.fs
include ../../flib/mecrisp/profiler.fs

: ta ;
: tb 0 do ta loop ;
: tc dup 0 do dup tb loop drop ;

see a
see ta

\ 100 tc
\ profile
