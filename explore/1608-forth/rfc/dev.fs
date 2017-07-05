\ try out remote console

compiletoram? [if]  forgetram  [then]

8686 rf.freq !
17 rf.group !

66 buffer: rfout

: go
  rf-init 16 rf-power
  $63626100 rfout !
  begin
    123 .
    rfout 4 0 rf-send
    3000 ms
  key? until ;

go
