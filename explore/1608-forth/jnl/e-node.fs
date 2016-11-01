\ exploring a self-contained sensor node
\ needs core.fs
cr cr reset
cr

1 constant debug  \ 0 = send RF packets, 1 = display on serial port
10 constant rate  \ seconds between readings

\ -----------------------------------------------------------------------------
\ variable-int encoding, turns 64-bit ints into 1..10 bytes

: drshift ( ud|d n -- ud|d )  \ double right shift n bits
  0 do dshr loop ;

: <v ( - d ) 0 0 <# ;  \ prepare variable output
: d#v ( d -- )  \ add one 64-bit value to output
  over $80 or hold
  begin
    7 drshift
  2dup or while
    over $7F and hold
  repeat 2drop ;
: v> ( d -- caddr len ) #> ;  \ finish, then return buffer and length
: u#v ( u -- ) 0 d#v ;  \ add a 32-bit uint to output as varint, max 5 bytes

\ some definitions to build up and send a packet with varints

20 cells buffer: pkt.buf  \ room to collect up to 20 values for sending
      0 variable pkt.ptr  \ current position in this packet buffer

: u+> ( u -- ) pkt.ptr @ ! 4 pkt.ptr +! ;  \ append 32-bit value to packet
: u14+> ( u -- ) $3FFF and u+> ;           \ append 14-bit value to packet

: <pkt ( format -- ) pkt.buf pkt.ptr ! u+> ;  \ start collecting values
: pkt>rf ( -- )  \ broadcast the collected values as RF packet
  <v
    pkt.ptr @  begin  4 - dup @ u#v  dup pkt.buf u<= until  drop
  v> 0 rf-send ;

\ for example, to send a packet of type 123, with values 11, 2222, and 333333:
\   123 <pkt 11 u+> 2222 u+> 333333 u+> pkt>rf

\ -----------------------------------------------------------------------------

\ assumes that the BME280 and TSL4531 sensors are connected to PB6..PB7

: display ( h p t l v -- )
  . ." °Cx100, " . ." Pa, " . ." %RHx100, "  . ." lux, "  . ." °C, " . ." mV " ;

: go
  bme-init bme-calib tsl-init
  begin
    led-off
    only-msi  rate 0 do stop1s loop  hsi-on
    +adc adc-vcc adc-temp -adc  tsl-data  bme-data bme-calc
    led-on
    debug if
      hwid hex. ." = " display cr 1 ms
    else
      2 <pkt hwid u+> u14+>  5 0 do u+> loop pkt>rf rf-sleep
    then
  key? until ;

2.1MHz 1000 systick-hz

8688 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
rf69-init rf-sleep

+lptim +i2c

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
