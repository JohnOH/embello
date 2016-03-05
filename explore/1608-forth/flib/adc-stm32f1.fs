\ simple one-shot ADC

$40012400 constant ADC1
    ADC1 $00 + constant ADC1-SR
    ADC1 $08 + constant ADC1-CR2
    ADC1 $34 + constant ADC1-SQR3
    ADC1 $4C + constant ADC1-DR

: init-adc ( -- )  \ initialise ADC
  9 bit RCC-APB2ENR bis!  \ set ADC1EN
  1 ADC1-CR2 bis!  \ set ADON to enable ADC
;

: adc ( pin - u )  \ read ADC value
  IMODE-ADC over io-mode!
\ nasty way to map the pins (a "c," table offset lookup might be simpler)
\   PA0..7 => 0..7, PB0..1 => 8..9, PC0..5 => 10..15
  dup io# swap  io-port ?dup if shl + 6 + then
    ADC1-SQR3 !
  1 ADC1-CR2 bis!  \ set ADON to start ADC
  begin 1 bit ADC1-SR bit@ until  \ wait until EOC set
  ADC1-DR @ ;
