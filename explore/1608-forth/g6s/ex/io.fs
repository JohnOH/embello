forgetram

\ include ../flib/stm32f1/io-orig.fs
\ include ../flib/stm32f1/io.fs

     PA0 constant PIN
  100000 constant NUM
 NUM 5 / constant NUM/5

( start: ) here dup hex.

: read5x  PIN io@ drop PIN io@ drop PIN io@ drop PIN io@ drop PIN io@ drop ;
: write5x dup PIN io! dup PIN io! dup PIN io! dup PIN io! ( dup ) PIN io! ;

: one5x   1 PIN io! 1 PIN io! 1 PIN io! 1 PIN io! 1 PIN io! ;
: zero5x  0 PIN io! 0 PIN io! 0 PIN io! 0 PIN io! 0 PIN io! ;

: set5x   PIN ios! PIN ios! PIN ios! PIN ios! PIN ios! ;
: clear5x PIN ioc! PIN ioc! PIN ioc! PIN ioc! PIN ioc! ;
: xor5x   PIN iox! PIN iox! PIN iox! PIN iox! PIN iox! ;

( end, size: ) here dup hex. swap - .

: reads  NUM/5 0 do read5x    loop ;
: ons    NUM/5 0 do 1 write5x loop ;
: offs   NUM/5 0 do 0 write5x loop ;
: ones   NUM/5 0 do one5x     loop ;
: zeros  NUM/5 0 do zero5x    loop ;
: sets   NUM/5 0 do set5x     loop ;
: clears NUM/5 0 do clear5x   loop ;
: xors   NUM/5 0 do xor5x     loop ;

: times cr
  ." r "  micros reads  micros swap - .  \ pin io@
  ." w1 " micros ons    micros swap - .  \ v pin io! ( v=1 )
  ." w0 " micros offs   micros swap - .  \ v pin io! ( v=0 )
  ." o "  micros ones   micros swap - .  \ 0 pin io!
  ." z "  micros zeros  micros swap - .  \ 1 pin io!
  ." s "  micros sets   micros swap - .  \ pin ios!
  ." c "  micros clears micros swap - .  \ pin ioc!
  ." x "  micros xors   micros swap - .  \ pin iox!
;

: forever
  OMODE-PP PIN io-mode!
  begin
    PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! 
    PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! 
    PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! 
    PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! PIN ioc! PIN ios! 
  again ;

times
