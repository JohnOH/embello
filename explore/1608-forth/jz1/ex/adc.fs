\ adc experiment
\ needs core.fs
cr cr reset

\ include ../flib/stm32l0/adc.fs

\ pin definitions: PA0 = 0V, PA1 = ADC in, PA2 = 3.3V
\ works well with a trimpot inserted in a breadboard
omode-pp  pa0 io-mode!  pa0 ioc!
imode-adc pa1 io-mode!
omode-pp  pa2 io-mode!  pa2 ios!

adc-init adc?

: temp+vcc
  cr micros adc-temp micros rot - . ." µs: " . ." °C "
  cr micros adc-vcc micros rot - . ." µs: " . ." mV "
;

temp+vcc

: go
  adc-vcc
  begin
    cr micros pa1 adc micros rot - . ." µs " over 4095 */ . ." mV "
    500 ms
  key? until drop ;

\ this causes folie to timeout on include matching, yet still starts running
1234 ms go
