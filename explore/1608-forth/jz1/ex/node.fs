\ exploring a self-contained sensor node
\ needs core.fs
cr cr reset
cr

\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7

0 constant debug  \ 0 = send RF packets, 1 = display on serial port
10 constant rate  \ seconds between readings

: display ( vprev vcc tint lux humi pres temp -- )
  hwid hex. ." = "
  . ." °cC, " . ." Pa, " . ." %cRH, "
  . ." lux, "  . ." °C, " . ." => " . ." mV " ;

: send-packet ( vprev vcc tint lux humi pres temp -- )
  2 <pkt  hwid u+>  n+> 6 0 do u+> loop  pkt>rf ;

: go
  bme-init bme-calib tsl-init
  begin
    adc-vcc

    rf-sleep  adc-deinit only-msi  rate 0 do stop1s loop  hsi-on adc-init

    adc-vcc adc-temp
    tsl-data  bme-data bme-calc

    led-on
    debug if
      display cr 1 ms
    else
      send-packet
    then
    led-off 
  key? until ;

2.1MHz 1000 systick-hz

8686 rf.freq ! 6 rf.group ! 62 rf.nodeid ! rf-init

lptim-init i2c-init adc-init

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
