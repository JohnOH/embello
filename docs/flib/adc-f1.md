# ADC interface

[code]: stm32f1/adc.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f1/adc.fs">stm32f1/adc.fs</a>

This interfaces to the built-in 1 Msps 12-bit ADC of the STM32F1xx µC.

### API

[defs]: <> (adc-init adc-calib)
```
: adc-init ( -- )  \ initialise ADC
: adc-calib ( -- )  \ perform an ADC calibration cycle
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
