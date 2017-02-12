# ADC interface

[code]: stm32l0/adc.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32l0/adc.fs">stm32l0/adc.fs</a>

This interfaces to the built-in 1 Msps 12-bit ADC of the STM32L0xx µC.

### API

[defs]: <> (adc-init adc-calib adc-deinit)
```
: adc-init ( -- )  \ initialise ADC
: adc-calib ( -- )  \ perform an ADC calibration cycle
: adc-deinit ( -- )  \ de-initialise ADC
```

[defs]: <> (adc adc-once)
```
: adc ( pin -- u )  \ read ADC value 2x to avoid chip erratum
: adc-once ( -- u )  \ read ADC value once
```

[defs]: <> (adc-vcc adc-temp)
```
: adc-vcc ( -- mv )  \ measure current Vcc
: adc-temp ( -- degc )  \ measure chip temperature
```

### Examples

    adc-init
    adc .
    adc-vcc .
    adc-temp .
