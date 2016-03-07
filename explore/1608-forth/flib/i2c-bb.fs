\ bit-banged i2c driver
\ adapted from http://excamera.com/sphinx/article-forth-i2c.html

: +i2c ( -- )  \ initialise bit-banged I2C
  OMODE-PP SCL io-mode!
  OMODE-OD SDA io-mode!
;

: i2c-half ( -- )  \ half-cycle timing delay for I2C
\ 10 0 do loop ; \ approx 250 KHz?
  inline ; \ no half-cycle delay, max speed

: i2c-start ( -- )  \ with SCL high, change SDA from 1 to 0
  1 SDA io! i2c-half SCL ios! i2c-half 0 SDA io! i2c-half SCL ioc! ;
: i2c-stop  ( -- )  \ with SCL high, change SDA from 0 to 1
  0 SDA io! i2c-half SCL ios! i2c-half 1 SDA io! i2c-half ;

: b>i2c ( f -- )  \ send one I2C bit
  0<> SDA io! i2c-half SCL ios! i2c-half SCL ioc! ;
: i2c>b ( -- b )  \ receive one I2C bit
  SDA ios! i2c-half SCL ios! i2c-half SDA io@ SCL ioc! ;

: >i2c ( b -- nak )  \ send one byte
  8 0 do dup 128 and b>i2c 2* loop drop i2c>b ;
: i2c> ( nak -- b )  \ read one byte
  0 8 0 do 2* i2c>b + loop swap b>i2c ;

: i2c-tx ( addr -- nak )  \ start device send
  i2c-start shl >i2c ;
: i2c-rx ( addr -- nak )  \ start device receive
  i2c-start shl 1+ >i2c ;

\ RTC example, this is small enough to leave it in
\ nak's will be silently ignored

: rtc: ( reg -- ) \ common i2c preamble for RTC
  $68 i2c-tx drop >i2c drop ;
: rtc! ( v reg -- ) \ write v to RTC register
  rtc: >i2c drop i2c-stop ;
: rtc@ ( reg -- v ) \ read RTC register
  rtc: i2c-start $68 i2c-rx drop 1 i2c> i2c-stop ;

: i2c. ( -- )  \ scan and report all I2C devices on the bus
  128 0 do
    cr i h.2 ." :"
    16 0 do  space
      i j +  dup i2c-rx i2c-stop  if drop ." --" else h.2 then
    loop
  16 +loop ;
