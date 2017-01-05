\ application setup and main loop
\ detect and list all attached I2C devices periodically

0 constant DEBUG  \ 0 = show on LCD, 1 = show on serial
0 constant RADIO  \ 0 = no radio present, 1 = radio present

\ debug LEDs, connected to rightmost I/O pins on main header
PA0  constant LED1
PA1  constant LED2
PA2  constant LED3
PA3  constant LED4
PA11 constant LED5
PA12 constant LED6

: leds-pwm ( -- )  \ blink the attached LEDs, just because we can...
\ FIXME set alt function #2 on all PWM pins, should be moved inside pwm driver
  $00002222 LED1 io-base GPIO.AFRL + !

  \ various duty cycles at 2 Hz
  2 LED1 pwm-init  3500 LED1 pwm
  2 LED2 pwm-init  5500 LED2 pwm
  2 LED3 pwm-init  7500 LED3 pwm
  2 LED4 pwm-init  9500 LED4 pwm
;

: lcd-emit ( c -- )  \ switch the output to the OLED, cr's move to next line
  dup $0A = if drop
    s"                 "  \ dumb way to clear a line
    0 dup font-x !  font-y @  8 +  $38 and  dup font-y !
    drawstring
    0 font-x !
  else
    ascii>bitpattern drawcharacterbitmap
  then ;

\ single-digit hex output
: h.1 ( u -- ) $F and base @ hex swap  .digit emit  base ! ;

: i2c.short ( -- )  \ scan and report all I2C devices on the bus, short format
  128 0 do
    DEBUG i or if cr then
    i h.2 ." : "
    
    16 0 do
      i j +
      dup $08 < over $77 > or if drop space else
        dup i2c-addr  0 i2c-xfer  if drop ." -" else h.1 then
      then
    loop
  16 +loop ;

: radio-init ( -- )
  8686 rf.freq ! 6 rf.group ! 62 rf.nodeid !
  rf-init 16 rf-power rf-sleep ;

: main ( -- )
  leds-pwm  lcd-init show-logo
  OMODE-PP LED5 io-mode!  OMODE-PP LED6 io-mode!
  DEBUG 0= if  ['] lcd-emit hook-emit !  then
  RADIO if radio-init then

  begin
    500 ms
    cr 0 font-x ! 0 font-y ! clear
    LED5 ios!  i2c.short  LED5 ioc!
    LED6 ios!  display    LED6 ioc!
  key? until

  ['] serial-emit hook-emit !  show-logo ;
