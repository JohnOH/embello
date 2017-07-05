\ try out sines and square roots
\
\ sample output on F103 @ 72 MHz:
\   1> trig.fs 22: 1,00000000162981450557708740234375  ok.
\   1> trig.fs 23: -0,00000000069849193096160888671875  ok.
\   1> trig.fs 25: 0,70710678072646260261535644531250  ok.
\   1> trig.fs 26: 0,70710678095929324626922607421875  ok.
\   1> trig.fs 28: 7071  ok.
\   1> trig.fs 36: 85  ok.
\   1> trig.fs 37: 85  ok.
\   1> trig.fs 38: 84  ok.
\   1> trig.fs 39: 84  ok.
\   1> trig.fs 40: 84  ok.
\   1> trig.fs 48: 85  ok.
\   1> trig.fs 49: 224  ok.
\   1> trig.fs 50: 518803  ok.
\   1> trig.fs 51: 518819  ok.
\   1> trig.fs 52: 3281  ok.

forgetram

include ../../flib/mecrisp/sine.fs
include ../../flib/mecrisp/sqrt.fs

pi 4,0 f/ 2constant pi/4
     pi/4 2variable angle
 50000000 variable  bigval

pi/2 sine f.
pi/2 cosine f.

pi/4 sine f.
pi/4 cosine f.

50000000 sqrt .

: none0 micros swap 0 do                       loop micros swap - . ;
: none1 micros swap 0 do pi/4            2drop loop micros swap - . ;
: none2 micros swap 0 do pi/4     sine   2drop loop micros swap - . ;
: none3 micros swap 0 do pi/4     cosine 2drop loop micros swap - . ;
: none4 micros swap 0 do 50000000 sqrt   drop  loop micros swap - . ;

1000 none0
1000 none1
1000 none2
1000 none3
1000 none4

: time0 micros swap 0 do                       loop micros swap - . ;
: time1 micros swap 0 do angle 2@        2drop loop micros swap - . ;
: time2 micros swap 0 do angle 2@ sine   2drop loop micros swap - . ;
: time3 micros swap 0 do angle 2@ cosine 2drop loop micros swap - . ;
: time4 micros swap 0 do bigval @ sqrt   drop  loop micros swap - . ;

1000 time0
1000 time1
1000 time2
1000 time3
1000 time4
