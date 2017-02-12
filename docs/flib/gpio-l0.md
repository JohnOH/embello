# GPIO for STM32L0xx µCs

[code]: stm32l0/io.fs (hal)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32l0/io.fs">stm32l0/io.fs</a>
* Needs: hal

These primitives are for using General Purpose I/O pins on STM32L0xx.

### API

[defs]: <> (io-mode! io.)
```
: io-mode! ( mode pin -- )  \ set the CNF and MODE bits for a pin
: io. ( pin -- )  \ display readable GPIO registers associated with a pin
```

[defs]: <> (io@ ioc! ios! iox! io!)
```
: io@ ( pin -- u )  \ get pin value (0 or -1)
: ioc! ( pin -- )  \ clear pin to low
: ios! ( pin -- )  \ set pin to high
: iox! ( pin -- )  \ toggle pin
: io! ( f pin -- )  \ set pin value
```

[defs]: <> (io io# io-mask io-port io-base)
```
: io ( port# pin# -- pin )  \ combine port and pin into single int
: io# ( pin -- u )  \ convert pin to bit position
: io-mask ( pin -- u )  \ convert pin to bit mask
: io-port ( pin -- u )  \ convert pin to port number (A=0, B=1, etc)
: io-base ( pin -- addr )  \ convert pin to GPIO base address
```

### Constants

These constants specify the pin configuration for `io-mode!`:

[defs]: <> (IMODE-ADC IMODE-FLOAT IMODE-HIGH IMODE-LOW)
```
%0001100 constant IMODE-ADC    \ input, analog
%0000000 constant IMODE-FLOAT  \ input, floating
%0010000 constant IMODE-HIGH   \ input, pull up
%0100000 constant IMODE-LOW    \ input, pull down
```

[defs]: <> (OMODE-PP OMODE-OD OMODE-AF-PP OMODE-AF-OD)
```
%0000110 constant OMODE-PP     \ output, push-pull
%1000110 constant OMODE-OD     \ output, open drain
%0001010 constant OMODE-AF-PP  \ alternate function, push-pull
%1001010 constant OMODE-AF-OD  \ alternate function, open drain
```

[defs]: <> (OMODE-WEAK OMODE-SLOW OMODE-FAST)
```
-2 constant OMODE-WEAK  \ add to OMODE-* for 400 KHz iso 10 MHz drive
-1 constant OMODE-SLOW  \ add to OMODE-* for 2 MHz iso 10 MHz drive
 1 constant OMODE-FAST  \ add to OMODE-* for 35 MHz iso 10 MHz drive
```
