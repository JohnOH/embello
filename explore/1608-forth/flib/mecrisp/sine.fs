\ Trigonometric functions with CORDIC algorithm
\ copied with minor mods from Mecrisp Stellaris "common/sine.txt"

\ -----------------------------------------------------------------------------
\   Constant table necessary for all CORDIC computations
\ -----------------------------------------------------------------------------

create cordic-const
hex
  C90FDAA2 , 76B19C15 , 3EB6EBF2 , 1FD5BA9A ,
  0FFAADDB , 07FF556E , 03FFEAAB , 01FFFD55 ,
  00FFFFAA , 007FFFF5 , 003FFFFE , 001FFFFF ,
  000FFFFF , 0007FFFF , 0003FFFF , 0001FFFF ,
  0000FFFF , 00007FFF , 00003FFF , 00001FFF ,
  00000FFF , 000007FF , 000003FF , 000001FF ,
  000000FF , 0000007F , 0000003F , 0000001F ,
  0000000F , 00000007 , 00000003 , 00000001 ,
decimal

cordic-const constant cordic-constants \ Allow stronger optimisations for the table address

: e^ka ( u -- x ) 2 lshift cordic-constants + @  1-foldable ;

\ -----------------------------------------------------------------------------
\   Common building blocks for different CORDIC modes
\ -----------------------------------------------------------------------------

0. 2variable cordic-x
0. 2variable cordic-y
0. 2variable cordic-z

0. 2variable cordic-x'

: 2arshift ( d u -- d* ) 0 ?do d2/ loop 3-foldable ;

: cordic-step-plus ( -- )
  cordic-x 2@ cordic-y 2@ i 2arshift d+ cordic-x' 2!
  cordic-y 2@ cordic-x 2@ i 2arshift d- cordic-y  2!
  cordic-z 2@             i e^ka 0   d+ cordic-z  2!

  cordic-x' 2@ cordic-x 2!
;

: cordic-step-minus ( -- )
  cordic-x 2@ cordic-y 2@ i 2arshift d- cordic-x' 2!
  cordic-y 2@ cordic-x 2@ i 2arshift d+ cordic-y  2!
  cordic-z 2@             i e^ka 0   d- cordic-z  2!

  cordic-x' 2@ cordic-x 2!
;

\ -----------------------------------------------------------------------------
\   Angle --> Sine and Cosine
\ -----------------------------------------------------------------------------

: cordic-sincos ( f-angle -- f-cosine f-sine )
                ( Angle between -Pi/2 and +Pi/2 ! )

  $0,9B74EDA8  cordic-x 2! \ Scaling value to cancel gain of the algorithm
   0,0         cordic-y 2!
               cordic-z 2!

  32 0 do
    cordic-z 2@ d0<
    if
      cordic-step-plus
    else
      cordic-step-minus
    then
  loop

  cordic-x 2@
  cordic-y 2@

2-foldable ;

: cosine ( f-angle -- f-cosine ) cordic-sincos 2drop  2-foldable ;
: sine   ( f-angle -- f-sine )   cordic-sincos 2nip   2-foldable ;

\ -----------------------------------------------------------------------------
\   Range extension for Sine and Cosine
\ -----------------------------------------------------------------------------

3,141592653589793  2constant pi
pi 2,0 f/          2constant pi/2

: widecosine ( f-angle -- f-cosine )
  dabs
  pi/2 ud/mod drop 3 and ( Quadrant f-angle )

  case
    0 of                 cosine         endof
    1 of dnegate pi/2 d+ cosine dnegate endof
    2 of                 cosine dnegate endof
    3 of dnegate pi/2 d+ cosine         endof
  endcase

2-foldable ;

: widesine ( f-angle -- f-sine )
  dup >r \ Save sign
  dabs
  pi/2 ud/mod drop 3 and ( Quadrant f-angle )

  case
    0 of                 sine          endof
    1 of dnegate pi/2 d+ sine          endof
    2 of                 sine  dnegate endof
    3 of dnegate pi/2 d+ sine  dnegate endof
  endcase

  r> 0< if dnegate then

2-foldable ;


\ -----------------------------------------------------------------------------
\  Integer XY vector --> Polar coordinates
\ -----------------------------------------------------------------------------

: cordic-vectoring ( x y -- d-r f-angle )

  \ The CORDIC algorithm on its own works fine with angles between -Pi/2 ... +Pi/2.
  \ Need to handle angles beyond, which translates to if x < 0,  by an additional step:

  over 0<  \ x < 0 ?
  if
    negate swap negate swap

    dup 0< if \ Now y < 0 ?
      pi         cordic-z 2!
    else
      pi dnegate cordic-z 2!
    then
  else
    0,0  cordic-z 2!
  then

  \ Improve accuracy by exploiting 64 bit dynamic range during calculations

  s>d 24 0 do d2* loop cordic-y 2!
  s>d 24 0 do d2* loop cordic-x 2!

  32 0 do

    cordic-y 2@ d0<
    if
      cordic-step-minus
    else
      cordic-step-plus
    then

  loop

  cordic-z 2@
  cordic-x 2@

2-foldable ;

: atan2     ( x y -- f-angle )             cordic-vectoring 2drop                2-foldable ;
: magnitude ( x y -- d-magnitude )         cordic-vectoring 2nip  $0,9B74EDA8 f* 24 2arshift 2-foldable ;
: xy>polar  ( x y -- f-angle d-magnitude ) cordic-vectoring       $0,9B74EDA8 f* 24 2arshift 2-foldable ;

\ -----------------------------------------------------------------------------
\   Small helpers for calculations
\ -----------------------------------------------------------------------------

: s>f ( n -- f ) 0 swap  inline 1-foldable ; \ Signed integer --> Fixpoint s31.32
: f>s ( f -- n ) nip     inline 2-foldable ; \ Fixpoint s31.32 --> Signed integer
