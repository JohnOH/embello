forgetram

\ include ../flib/stm32f1/io-orig.fs
include ../flib/stm32f1/io.fs

     PA0 constant PIN
  100000 constant NUM
 NUM 5 / constant NUM/5

( start: ) here dup hex.

: read5x   PIN io@ drop PIN io@ drop PIN io@ drop PIN io@ drop PIN io@ drop ;

: set5x    PIN ios! PIN ios! PIN ios! PIN ios! PIN ios! ;
: clear5x  PIN ioc! PIN ioc! PIN ioc! PIN ioc! PIN ioc! ;
: toggle5x PIN iox! PIN iox! PIN iox! PIN iox! PIN iox! ;

: zero5x   0 PIN io! 0 PIN io! 0 PIN io! 0 PIN io! 0 PIN io! ;
: one5x    1 PIN io! 1 PIN io! 1 PIN io! 1 PIN io! 1 PIN io! ;

( end, size: ) here dup hex. swap - .

: reads   NUM/5 0 do read5x   loop ;
: sets    NUM/5 0 do set5x    loop ;
: clears  NUM/5 0 do clear5x  loop ;
: toggles NUM/5 0 do toggle5x loop ;
: zeros   NUM/5 0 do zero5x   loop ;
: ones    NUM/5 0 do one5x    loop ;

: times cr
  ." read: "   micros reads   micros swap - .
  ." set: "    micros sets    micros swap - .
  ." clear: "  micros clears  micros swap - .
  ." toggle: " micros toggles micros swap - .
  ." zero: "   micros zeros   micros swap - .
  ." one: "    micros ones    micros swap - .
;

times
