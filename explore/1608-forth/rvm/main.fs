\ application setup and main loop
\ assumes that the Analog Plug is connected to PB6..PB7

0 constant debug  \ 0 = send RF packets, 1 = display on serial port

: show-readings ( vy vb vg vr -- )
  hwid hex. ." = "
  ." Vr: " . ." Vg: " . ." Vb " . ." Vy " . ;

: send-packet ( vy vb vg vr -- )
  3 <pkt  hwid u+>  4 0 do n+> loop  pkt>rf ;

: main
  2.1MHz  1000 systick-hz  +lptim +i2c +adc

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power

  mcp-init if ." can't find MCP3424" exit then

  begin
    led-off rf-sleep

    3 mcp-data 2 mcp-data 1 mcp-data 0 mcp-data

    led-on
    debug if
      hsi-on show-readings cr 1 ms
    else
      send-packet
    then
  key? until ;
