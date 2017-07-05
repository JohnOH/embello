\ try out the trace utility
\
\ sample output, without tracing:
\   tc 1 2 3 4 5  ok.
\
\ sample output, with tracing:
\   tc    --> ( 1 ) --> tc  Stack: [0 ]  TOS: 42  *>
\   1    --> ( 2 ) --> ta  Stack: [0 ]  TOS: 42  *>
\   2    <-- ( 2 ) Stack: [0 ]  TOS: 42  *>
\   3    --> ( 2 ) --> tb  Stack: [0 ]  TOS: 42  *>
\   4    <-- ( 2 ) Stack: [0 ]  TOS: 42  *>
\   5    <-- ( 1 ) Stack: [0 ]  TOS: 42  *>
\    ok.
\
\ see a
\ 20000394: F847  str r6 [ r7 #-4 ]!
\ 20000396: 6D04
\ 20000398: 2602  movs r6 #2
\ 2000039A: B500  push { lr }
\ 2000039C: F244  movw r0 #434F
\ 2000039E: 304F
\ 200003A0: 4780  blx r0  --> .
\ 200003A2: BD00  pop { pc }
\  ok.
\ 
\ see ta
\ 20002892: B500  push { lr }
\ 20002894: F7FF  bl  2000269E  --> trace-entry
\ 20002896: FF03
\ 20002898: F847  str r6 [ r7 #-4 ]!
\ 2000289A: 6D04
\ 2000289C: 2602  movs r6 #2
\ 2000289E: F244  movw r0 #434F
\ 200028A0: 304F
\ 200028A2: 4780  blx r0  --> .
\ 200028A4: F7FF  bl  20002736  --> trace-exit
\ 200028A6: FF47
\ 200028A8: BD00  pop { pc }

forgetram

: a 2 . ;
: b 4 . ;
: c 1 . a 3 . b  5 . ;

include ../../flib/mecrisp/disassembler-m3.fs
include ../../flib/mecrisp/trace.fs

: ta 2 . ;
: tb 4 . ;
: tc 1 . ta 3 . tb  5 . ;

see a
see ta

\ tc
\ trace-on
\ tc
