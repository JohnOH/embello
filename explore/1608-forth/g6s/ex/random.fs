\ try out the pseudo-random generator
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

forgetram

include ../../flib/mecrisp/random.fs

16 cells buffer: buckets
buckets 16 cells 0 fill

: tally ( n -- )
  0 do
    random
    32 0 do 1 over i rshift $F and cells buckets + +! 4 +loop
    drop
  loop ;

: counts 16 0 do i cells buckets + @ cr . loop ;

\ 2000000 tally counts
