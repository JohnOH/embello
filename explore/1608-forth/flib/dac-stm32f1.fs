\ DAC output

$40007400 constant DAC
     DAC $00 + constant DAC-CR
\    DAC $04 + constant DAC-SWTRIGR
\    DAC $08 + constant DAC-DHR12R1
\    DAC $14 + constant DAC-DHR12R2
     DAC $20 + constant DAC-DHR12RD

: 2dac! ( u1 u2 -- )  \ send values to each of the DACs
  16 lshift or DAC-DHR12RD ! ;

: dac-init ( -- )  \ initialise the two D/A converters on PA4 and PA5
  29 bit RCC-APB1ENR bis!  \ DACEN clock enable
  IMODE-ADC PA4 io-mode!
  IMODE-ADC PA5 io-mode!
  $00010001 DAC-CR !  \ enable channel 1 and 2
  0 0 2dac!
;

: dac-triangle ( -- )  \ software-driven dual triangle waveform until keypress
  dac-init
  begin
    $1000 0 do  i          $FFF i - 2dac! loop
    $1000 0 do  $FFF i -   i        2dac! loop
  key? until
;

: dac1-noise ( u -- )  \ generate noise on DAC1 (PA4) with given period
  tim6-init dac-init
           0 bit     \ EN1
  %1011 8 lshift or  \ MAMP1 max
    %01 6 lshift or  \ WAVE1 = noise
                     \ TSEL1 = timer 6 TRGO
           2 bit or  \ TEN1
  DAC-CR !
;

: dac1-triangle ( u -- )  \ generate triangle on DAC1 (PA4) with given period
  tim6-init dac-init
           0 bit     \ EN1
  %1011 8 lshift or  \ MAMP1 max
    %10 6 lshift or  \ WAVE1 = noise
                     \ TSEL1 = timer 6 TRGO
           2 bit or  \ TEN1
  DAC-CR !
;
