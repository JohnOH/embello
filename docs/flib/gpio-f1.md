# GPIO for STM32F1xx µCs

[code]: stm32f1/io.fs (hal)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f1/io.fs">stm32f1/io.fs</a>
* Needs: hal

These primitives are for using General Purpose I/O pins on STM32F1xx.

### API

[defs]: <> (io-mode! io-modes! io.)
```
: io-mode! ( mode pin -- )  \ set the CNF and MODE bits for a pin
: io-modes! ( mode pin mask -- )  \ shorthand to config multiple pins of a port
: io. ( pin -- )  \ display readable GPIO registers associated with a pin
```

[defs]: <> (io@ ioc! ios! iox! io!)
```
: io@ ( pin -- f )  \ get pin value (0 or -1)
: ioc! ( pin -- )  \ clear pin to low
: ios! ( pin -- )  \ set pin to high
: iox! ( pin -- )  \ toggle pin, not interrupt safe
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

[defs]: <> (IMODE-ADC IMODE-FLOAT IMODE-PULL)
```
%0000 constant IMODE-ADC    \ input, analog
%0100 constant IMODE-FLOAT  \ input, floating
%1000 constant IMODE-PULL   \ input, pull-up/down
```

[defs]: <> (OMODE-PP OMODE-OD OMODE-AF-PP OMODE-AF-OD)
```
%0001 constant OMODE-PP     \ output, push-pull
%0101 constant OMODE-OD     \ output, open drain
%1001 constant OMODE-AF-PP  \ alternate function, push-pull
%1101 constant OMODE-AF-OD  \ alternate function, open drain
```

[defs]: <> (OMODE-SLOW OMODE-FAST)
```
%01 constant OMODE-SLOW  \ add to OMODE-* for 2 MHz iso 10 MHz drive
%10 constant OMODE-FAST  \ add to OMODE-* for 50 MHz iso 10 MHz drive
```
