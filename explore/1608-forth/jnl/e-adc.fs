\ adc experiment
\ needs core.fs
cr cr reset

include ../flib/adc-stm32l0.fs

\ pin definitions: PA0 = 0V, PA1 = ADC in, PA2 = 3.3V
\ works well for a trimpot inserted in a breadboard
 omode-pp pa0 io-mode!  pa0 ioc!
imode-adc pa1 io-mode!
 omode-pp pa2 io-mode!  pa2 ios!

: go
  +adc adc.
  begin
    cr micros pa1 adc micros rot - . .
    500 ms
  key? until ;

\ this causes folie to timeout on include matching, yet still starts running
\ 1234 ms go

+adc 

: go
  cr micros adc-temp micros rot - . ." µs " . ." °C "
  cr micros adc-vcc micros rot - . ." µs " . ." mV "
;

go
