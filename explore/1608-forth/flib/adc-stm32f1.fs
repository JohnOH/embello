\ simple one-shot ADC

RCC $18 + constant RCC-APB2ENR
         9 bit constant ADC1EN

$40012400 constant ADC1
    ADC1 $00 + constant ADC1-SR
         1 bit constant EOC
    ADC1 $08 + constant ADC1-CR2
         0 bit constant ADON
    ADC1 $34 + constant ADC1-SQR3
    ADC1 $4C + constant ADC1-DR

: init-adc ( -- )  \ initialise ADC
  ADC1EN RCC-APB2ENR bis!
  ADON ADC1-CR2 bis! ;

: adc ( pin - u )  \ read ADC value
  IMODE-ADC over io-mode!
\ nasty way to map the pins (a "c," table offset lookup might be simpler)
\   PA0..7 => 0..7, PB0..1 => 8..9, PC0..5 => 10..15
  dup io# swap  io-port ?dup if shl + 6 + then
    ADC1-SQR3 !
  ADON ADC1-CR2 bis!
  begin EOC ADC1-SR bit@ until
  ADC1-DR @ ;
