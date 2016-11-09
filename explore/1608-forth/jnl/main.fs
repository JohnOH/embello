\ application setup and main loop
\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
10 constant rate  \ seconds between readings

: display ( vprev vcc tint lux humi pres temp -- )
  hwid hex. ." = "
  . ." °cC, " . ." Pa, " . ." %cRH, "
  . ." lux, "  . ." °C, " . ." => " . ." mV " ;

: send-packet ( vprev vcc tint lux humi pres temp -- )
  2 <pkt  hwid u+>  u14+> 6 0 do u+> loop  pkt>rf ;

: low-power-sleep
  rf-sleep
  -adc \ only-msi
  rate 0 do stop1s loop
  hsi-on +adc ;

: main
  2.1MHz  1000 systick-hz  +lptim +i2c +adc

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf-power

  bme-init bme-calib  tsl-init

  begin
    led-off 

    adc-vcc                      ( vprev )
    low-power-sleep
    adc-vcc adc-temp             ( vprev vcc tint )
    tsl-data  bme-data bme-calc  ( vprev vcc tint lux humi pres temp )

    led-on

    debug if
      display cr 1 ms
    else
      send-packet
    then
  key? until ;
