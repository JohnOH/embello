\ Blink size debug LEDs, see http://jeelabs.org/article/1651d

forgetram

\ configure one pin as push-pull output
: out ( pin -- )  OMODE-PP swap io-mode! ;

\ configure all the LED pins as outputs
: setup  PA0 out  PA1 out  PA2 out  PA3 out  PA4 out  PA5 out ;

\ turn one pin on for 100 milliseconds
: blip ( pin -- )  dup ios!  100 ms  ioc! ;

\ blink LEDs in a loop, until new input is received from Folie
: go
  begin
    PA0 blip  PA1 blip  PA2 blip  PA3 blip  PA4 blip  PA5 blip
  key? until ;

setup go  \ press <enter> to quit
