\ hardware i2c driver

[ifndef] SCL  PB6 constant SCL  [then]
[ifndef] SDA  PB7 constant SDA  [then]

$40005400 constant I2C1
     I2C1 $00 + constant I2C1-CR1
     I2C1 $04 + constant I2C1-CR2
\    I2C1 $08 + constant I2C1-OAR1
\    I2C1 $0C + constant I2C1-OAR2
     I2C1 $10 + constant I2C1-TIMINGR
\    I2C1 $14 + constant I2C1-TIMEOUTR
     I2C1 $18 + constant I2C1-ISR
     I2C1 $1C + constant I2C1-ICR
\    I2C1 $20 + constant I2C1-PXCR
     I2C1 $24 + constant I2C1-RXDR
     I2C1 $28 + constant I2C1-TXDR

: i2c? ( -- )
  I2C1
  cr ."       CR1 " dup @ hex. 4 +
          ."  CR2 " dup @ hex. 4 +
         ."  OAR1 " dup @ h.4 space 4 +
         ."  OAR2 " dup @ h.4 space 4 +
      ."  TIMINGR " dup @ hex. 4 +
  cr ."  TIMEOUTR " dup @ hex. 4 +
          ."  ISR " dup @ hex. 4 +
         ."   ICR " dup @ h.4 space 4 +
         ."  PXCR " dup @ h.2 space 4 +
       ."    RXDR " dup @ h.2 space 4 +
         ."  TXDR " dup @ h.2 space drop ;

: +i2c ( -- )  \ initialise I2C hardware
  OMODE-AF-OD SCL io-mode!
  OMODE-AF-OD SDA io-mode!

  \ TODO this assumes PB6+PB7 and messes up the settings of the other pins
  $11000000 PB6 io-base GPIO.AFRL + !

  21 bit RCC-APB1ENR bis!  \ set I2C1EN
  $00300619 I2C1-TIMINGR !
  0 bit I2C1-CR1 bis!  \ PE
;

: ack-nak ( -- f )
  4 bit I2C1-ISR bit@ 0<>  \ NAKF
;

: wait-tcr ( -- )
  16 bit I2C1-CR2 bis!  \ NBYTES = 1
  begin 7 bit I2C1-ISR bit@ until  \ TCR
;

: i2c-stop  ( -- )
  14 bit I2C1-CR2 bis!  \ STOP
  begin 15 bit I2C1-ISR bit@ 0= until  \ !BUSY
;

: >i2c ( b -- nak )  \ send one byte
  I2C1-TXDR c! wait-tcr
  ack-nak ;

: i2c> ( nak -- b )  \ read one byte
  if 14 bit I2C1-CR2 bis! then  \ STOP
  wait-tcr I2C1-RXDR c@ ;

: i2c-rxtx ( addr rw -- f )
  9 lshift or shl $01012000  or I2C1-CR2 !
  begin 13 bit I2C1-CR2 bit@ 0= until  \ START
  ack-nak ;

: i2c-tx ( addr -- nak ) 0 i2c-rxtx ;  \ start device send
: i2c-rx ( addr -- nak ) 1 i2c-rxtx ;  \ start device receive

: i2c. ( -- )  \ scan and report all I2C devices on the bus
  128 0 do
    cr i h.2 ." :"
    16 0 do  space
      i j +
      dup $08 < over $77 > or if drop 2 spaces else
        dup i2c-tx i2c-stop  if drop ." --" else h.2 then
      then
    loop
  16 +loop ;
