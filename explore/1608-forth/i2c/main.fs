\ application setup and main loop

: main
  +i2c debug-pwm

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power rf-sleep

  begin
    i2c.
    1000 ms
  key? until ;
