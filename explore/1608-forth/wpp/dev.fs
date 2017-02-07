forgetram

PC13 constant LED

PB0 constant PEN

PA0 constant M1A
PA1 constant M1B
PA2 constant M1C
PA3 constant M1D

PA4 constant M2A
PA5 constant M2B
PA6 constant M2C
PA7 constant M2D

: app-setup
  OMODE-PP LED io-mode!  LED ios!
  OMODE-PP M1A io-mode!
  OMODE-PP M1B io-mode!
  OMODE-PP M1C io-mode!
  OMODE-PP M1D io-mode!
  OMODE-PP M2A io-mode!
  OMODE-PP M2B io-mode!
  OMODE-PP M2C io-mode!
  OMODE-PP M2D io-mode!
  50 PEN pwm-init  1000 PEN pwm
;

: step ( pin -- ) dup ios! 1000 us ioc! 1000 us ;

: forward1 M1A step M1B step M1C step M1D step ;
: forward2 M2A step M2B step M2C step M2D step ;
: reverse1 M1D step M1C step M1B step M1A step ;
: reverse2 M2D step M2C step M2B step M2A step ;

: rotate
  begin
    forward1
\   512 0 do forward1 loop
\   500 ms
\   reverse2
  key? until ;

: pen-up    500  50 0 do 10 + dup PEN pwm 10 ms loop  drop ;
: pen-down 1000  50 0 do 10 - dup PEN pwm 10 ms loop  drop ;

: pen-cycle
  500  begin
    pen-down
    LED ioc!  1000 ms  LED ios!
    pen-up
    500 ms
  key? until  drop ;

app-setup
