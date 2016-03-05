\ DAC output

$40007400 constant DAC
     DAC $00 + constant DAC-CR
\    DAC $08 + constant DAC-DHR12R1
\    DAC $14 + constant DAC-DHR12R2
     DAC $20 + constant DAC-DHR12RD

: 2dac! ( u1 u2 -- )  \ send values to each of the DACs
  16 lshift or DAC-DHR12RD ! ;

: dac-init ( -- )
  29 bit RCC-APB1ENR bis!  \ set DACEN
  IMODE-ADC PA4 io-mode!
  IMODE-ADC PA5 io-mode!
  $00010001 DAC-CR !  \ enable channel 1 and 2
  0 0 2dac!
;

: triangle
  dac-init
  begin
    $1000 0 do  i          $FFF i - 2dac! loop
    $1000 0 do  $FFF i -   i        2dac! loop
  key? until
;
