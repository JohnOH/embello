# GPIO for STM32F4xx µCs

[code]: stm32f4/io.fs (hal)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f4/io.fs">stm32f4/io.fs</a>
* Needs: hal

These primitives are for using General Purpose I/O pins on STM32F4xx.

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

[defs]: <> (IMODE-ADC IMODE-FLOAT IMODE-LOW IMODE-HIGH)
```
%0001100 constant IMODE-ADC    \ input, analog
%0000000 constant IMODE-FLOAT  \ input, floating
%0100000 constant IMODE-LOW    \ input, pull down
%0010000 constant IMODE-HIGH   \ input, pull up
```

[defs]: <> (OMODE-PP OMODE-OD OMODE-OD-HIGH OMODE-AF-PP OMODE-AF-OD OMODE-AF-OD-HIGH)
```
%0000110 constant OMODE-PP     \ output, push-pull
%1000110 constant OMODE-OD     \ output, open drain
%1010110 constant OMODE-OD-HIGH  \ output, open drain, pull up
%0001010 constant OMODE-AF-PP  \ alternate function, push-pull
%1001010 constant OMODE-AF-OD  \ alternate function, open drain
%1011010 constant OMODE-AF-OD-HIGH  \ alternate function, open drain, pull up
```

[defs]: <> (OMODE-SLOW OMODE-FAST)
```
-1 constant OMODE-SLOW  \ add to OMODE-* for 2 MHz iso 10 MHz drive
 1 constant OMODE-FAST  \ add to OMODE-* for 35 MHz iso 10 MHz drive
```
