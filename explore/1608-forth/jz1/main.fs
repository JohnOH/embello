\ application setup and main loop
\ assumes that the BME280 sensor is connected to PB6..PB7

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
10 constant rate  \ seconds between readings

: .00 ( n -- ) 0 swap 0,5 d+ 0,01 f* 2 f.n ;

: show-readings ( vprev vcc tint humi pres temp -- )
  hwid hex. ." = "
  .00 ." °C, " .00 ." hPa, " .00 ." %RH, "
  . ." °C, " . ." => " . ." mV " ;

: send-packet ( vprev vcc tint humi pres temp -- )
  2 <pkt  hwid u+>  n+> 5 0 do u+> loop  pkt>rf ;

: low-power-sleep
  rf-sleep
  adc-deinit \ only-msi
  rate 0 do stop1s loop
  hsi-on adc-init ;

: main
  2.1MHz  1000 systick-hz  lptim-init i2c-init adc-init

  8686 rf.freq ! 6 rf.group ! 62 rf.nodeid !
  rf-init 16 rf-power

  bme-init drop bme-calib

  begin
    led-off 

    adc-vcc            ( vprev )
    low-power-sleep
    adc-vcc adc-temp   ( vprev vcc tint )
    bme-data bme-calc  ( vprev vcc tint humi pres temp )

    led-on

    debug if
      show-readings cr 1 ms
    else
      send-packet
    then
  key? until ;
