\ read out the TSL4531 sensor
\ needs i2c

: tsl-rd ( reg -- )
  $29 i2c-tx drop
  >i2c drop i2c-stop
  $29 i2c-rx drop ;

: tsl-init ( -- ) +i2c $29 i2c-tx drop $03 >i2c drop i2c-stop ;
: tsl-data ( -- v ) $84 tsl-rd 0 i2c> 1 i2c> i2c-stop 8 lshift or ;

\ tsl-init
\ tsl-data .
