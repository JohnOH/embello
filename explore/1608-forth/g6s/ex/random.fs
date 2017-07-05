\ try out the pseudo-random number generator
\
\ sample output:
\   2000000 tally counts
\   999707
\   999541
\   1001010
\   1001118
\   999600
\   999359
\   1000272
\   999002
\   999779
\   1000194
\   999404
\   1000235
\   1000026
\   1000394
\   1000672
\   999687  ok.
\
\   time 542006  ok.

forgetram

include ../../flib/mecrisp/random.fs

16 cells buffer: buckets

: ++count ( n -- )  cells buckets +  1 swap +! ;

: tally ( n -- )  \ count all the 4-bit nibbles of N 32-bit random numbers
  buckets 16 cells 0 fill
  0 do
    random
    32 0 do
      dup i rshift $F and
      ++count
    4 +loop
    drop
  loop ;

: tally2 ( n -- )  \ count N random numbers in the range 0..15
  buckets 16 cells 0 fill
  0 do
    16 randrange ++count
  loop ;

: counts ( -- )  \ display the 16 aggregated counts
  16 0 do i cells buckets + @ cr . loop ;

: time micros 1000000 0 do random drop loop micros swap - . ;

\ 2000000 tally counts
\ 16000000 tally2 counts
\ time
