# ADC interface

[code]: stm32f4/adc.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f4/adc.fs">stm32f4/adc.fs</a>

This interfaces to the built-in 1 Msps 12-bit ADC of the STM32F4xx µC.

### API

[defs]: <> (adc-init adc-calib)
```
: adc-init ( -- )  \ initialise ADC
: adc-calib ( -- )  \ not needed on F4, retained for compatibility
```

[defs]: <> (adc adc-once)
```
: adc ( pin -- u )  \ read ADC value
: adc-once ( -- u )  \ read ADC value once
```

[defs]: <> (adc-vcc)
```
: adc-vcc ( -- mv )  \ return estimated Vcc, based on 1.2V internal bandgap
```

### Examples

    adc-init
    adc .
    adc-vcc .
