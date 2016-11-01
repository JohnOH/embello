\ exploring a self-contained sensor node
\ needs core.fs
cr cr reset
cr

\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7

: display ( h p t l v -- )
  . ." mV "  . ." lux "  . ." °Cx100 " . ." Pa " . ." %RHx100 " ;

: go
  bme-init bme-calib tsl-init
  begin
    only-msi stop1s stop1s stop1s hsi-on
    bme-data bme-calc  tsl-data  +adc adc-vcc -adc
    display cr 1 ms
  key? until ;

led-off
rf69-init rf-sleep
2.1MHz 1000 systick-hz
+lptim +i2c

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
