\ try out the routes utility

forgetram

: a 2 . ;
: b 4 . ;
: c 1 . a 3 . b  5 . ;

include ../../flib/mecrisp/disassembler-m3.fs
include ../../flib/mecrisp/routes.fs

: ta 2 . ;
: tb 4 . ;
: tc 1 . ta 3 . tb  5 . ;
: td ;

see a
see ta
see td

\ tc
\ : ta' 22 . ; route ta ta'
\ tc
