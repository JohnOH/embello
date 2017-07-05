\ read out the TSL4531 sensor
\ needs i2c

: tsl-addr $29 i2c-addr ;                 \ set the device i2c address
: tsl-reg ( n -- ) tsl-addr $80 or >i2c ; \ select register n
: tsl-ctrl ( -- ) 0 tsl-reg ;             \ select control register
: tsl-dreg ( -- ) 4 tsl-reg ;             \ select data register

: tsl-init ( -- nak ) \ put device into normal mode, 400ms integration
  i2c-init tsl-ctrl
  $03 >i2c \ control: normal mode
  \ $00 >i2c \ config: 400ms integration, power-save
  0 i2c-xfer ;

: tsl-data ( -- v ) \ read data
  tsl-dreg
  2 i2c-xfer drop
  i2c>h ;

: tsl-sleep ( -- ) \ put device to sleep
  tsl-ctrl
  $00 >i2c \ control: sleep mode
  0 i2c-xfer drop ;

: tsl-convert ( -- ms ) \ one-shot conversion, returns time to sleep before tsl-data
  tsl-ctrl
  $02 >i2c \ control: one-shot conversion
  0 i2c-xfer drop
  400 ;

\ tsl-init .
\ tsl-data .
