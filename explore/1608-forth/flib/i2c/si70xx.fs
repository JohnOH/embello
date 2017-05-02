
: Si7021-RH-conv ( u -- s ) \ converts measured RH to signed int RH
  125 * 65536 / 6 - ;
: Si7021-T-conv ( u -- ds) \ converts measured T to fixed point T
  0 swap	( make it fixed point)
  72 175 f*	( * 175.72 )
  0 65536 f/	( wasteful, but who cares )
  85 46 d-	( - 46.85 )
  ;
: Si-RH      $40 i2c-addr $E5 >i2c 2 i2c-xfer i2c>h_inv Si7021-RH-conv ; \ Measure RH (wait)
: Si-T       $40 i2c-addr $E3 >i2c 2 i2c-xfer i2c>h_inv Si7021-T-conv ;  \ Measure T (wait)
: Si-lastT   $40 i2c-addr $E0 >i2c 2 i2c-xfer i2c>h_inv Si7021-T-conv ; \ Get T from last RH measurement
: Si-serial1 ( -- u) \ Gets first 32 bits of serial
  $40 i2c-addr $FA >i2c $0F >i2c
  8 i2c-xfer
  0
  4 0 do
    i2c> i2c> drop
    swap 8 lshift or
  loop ;

: Si-serial2 ( -- u) \ Gets 2nd 32 bits of serial
  $40 i2c-addr $FC >i2c $C9 >i2c
  6 i2c-xfer
  0
  2 0 do
    i2c>h_inv i2c> drop
    swap 16 lshift or
  loop ;
