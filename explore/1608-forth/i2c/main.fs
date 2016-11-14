\ application setup and main loop
\ detect and list all attached I2C devices periodically

1 constant DEBUG  \ 0 = show on LCD, 1 = show on serial

: lcd-emit ( c -- )
  dup $0A = if drop
    s"                 "  \ dumb way to clear a line
    0 dup font-x !  font-y @  8 +  $38 and  dup font-y !
    drawstring
    0 font-x !
  else
    ascii>bitpattern drawcharacterbitmap
  then ;

: h.1 ( u -- ) $F and base @ hex swap  .digit emit  base ! ;

: i2c.lcd ( -- )  \ scan and report all I2C devices on the bus
  128 0 do
    [ DEBUG 0= ] [if]  i 0= if clear then  [else]  cr i h.2 ." : "  [then]
    
    16 0 do
      i j +
      dup $08 < over $77 > or if drop space else
        dup i2c-tx i2c-stop  if drop ." -" else h.1 then
      then
    loop
  16 +loop ;

: main
  +i2c debug-pwm
  [ DEBUG 0= ] [if]  lcd-init  ['] lcd-emit hook-emit !  [then]

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power rf-sleep

  begin
    i2c.lcd display
    1000 ms
  key? until ;
