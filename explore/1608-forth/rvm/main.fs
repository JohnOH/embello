\ application setup and main loop
\ assumes that the Analog Plug is connected to PB6..PB7

0 constant debug  \ 0 = send RF packets, 1 = display on serial port

: show-readings ( vy vb vg vr -- )
  hwid hex. ." = "
  ." Vr: " . ." Vg: " . ." Vb " . ;

: send-packet ( vy vb vg vr -- )
  3 <pkt  hwid u+>  3 0 do n+> loop  pkt>rf ;

: opamp-on
  VCC1 ios!  VCC2 ios!  \ tied together: must always be the same!
  OMODE-PP VCC1 io-mode!  OMODE-PP VCC2 io-mode! ;

: adc-pins
  IMODE-ADC ANA1 io-mode!
  IMODE-ADC ANA2 io-mode!
  IMODE-ADC ANA3 io-mode!
  IMODE-ADC ANA4 io-mode! ;

: main
  2.1MHz 1000 systick-hz  lptim-init opamp-on adc-pins

  8686 rf.freq ! 6 rf.group ! 62 rf.nodeid !
  rf-init 16 rf-power

  mcp-init if ." can't find MCP3424" exit then

  begin
    led-off rf-sleep

    0 mcp-data 2 mcp-data 3 mcp-data  \ op-amp on chan #1 is not working!

    led-on
    debug if
      hsi-on show-readings cr 1 ms
    else
      send-packet
    then
  key? until ;
