\ launch embedded SerPlus code from Forth

$20002000 constant SERPLUS.ADDR
$E000ED08 constant VTOR

: systick-off ( -- ) 0 $E000E010 ! ;

: serplus
  SERPLUS.DATA SERPLUS.ADDR SERPLUS.SIZE move  \ copy to RAM

  ." Switching to SerPlus..."
  10 0 do usb-poll 1 ms loop  \ drain USB connection

  23 bit RCC-APB1ENR bic!  \ clear USBEN
  0 bit $4001080C bis!  \ set PA0 high
  10 ms  \ force USB disconnect

  systick-off  \ can't have any interrupts firing from now on
  SERPLUS.ADDR VTOR !  \ reset the vector base to RAM

  SERPLUS.ADDR @ rp!  \ set the new stack location
  SERPLUS.ADDR cell+ @ execute \ jump to start of code
;
