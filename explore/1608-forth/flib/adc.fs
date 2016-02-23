\ simple one-shot ADC

RCC $18 + constant RCC.APB2ENR
    1 9 lshift constant ADC1EN

$40012400 constant ADC1
    ADC1 $00 + constant ADC1.SR
    1 1 lshift constant EOC
    ADC1 $08 + constant ADC1.CR2
    1 0 lshift constant ADON
    ADC1 $34 + constant ADC1.SQR3
    ADC1 $4C + constant ADC1.DR

: init-adc ( -- )  \ initialise ADC
  ADC1EN RCC.APB2ENR bis!
  ADON ADC1.CR2 bis! ;

: adc ( pin - u )  \ read ADC value
  IMODE-ADC over io-mode!
  \ TODO only implemented for A0..7 and B0..1
  dup io# swap io-port if 8 + then ADC1.SQR3 !
  ADON ADC1.CR2 bis!
  begin EOC ADC1.SR bit@ until
  ADC1.DR @
;
