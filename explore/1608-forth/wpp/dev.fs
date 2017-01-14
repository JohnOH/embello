forgetram

PC13 constant LED

PA0 constant M1A
PA1 constant M1B
PA2 constant M1C
PA3 constant M1D

PA4 constant M2A
PA5 constant M2B
PA6 constant M2C
PA7 constant M2D

: app-setup
  OMODE-PP LED io-mode!
  OMODE-PP M1A io-mode!
  OMODE-PP M1B io-mode!
  OMODE-PP M1C io-mode!
  OMODE-PP M1D io-mode!
  OMODE-PP M2A io-mode!
  OMODE-PP M2B io-mode!
  OMODE-PP M2C io-mode!
  OMODE-PP M2D io-mode!
;

: delay 1500 us ;

: step ( pin -- ) dup ios! delay ioc! ;

: forward1 M1A step M1B step M1C step M1D step ;
: forward2 M2A step M2B step M2C step M2D step ;
: reverse1 M1D step M1C step M1B step M1A step ;
: reverse2 M2D step M2C step M2B step M2A step ;

: rotate
  begin
\   forward1
    reverse2
  key? until ;

app-setup
