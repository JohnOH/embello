# Forth Library Documentation

This information applies to the
[explore/1608-forth/flib/](https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib)
area on GitHub.
The source of this documentation is found at
[documentation source](https://github.com/jeelabs/embello/tree/master/docs/flib/),
and the markdown text there is spliced together with comments extracted from the source
code using the 
using the [docex](https://github.com/jeelabs/embello/tree/master/tools/docex/) tool.

| STM32L0 | STM32F1 | |
| --- | --- | --- |
| [adc-l0](adc-l0.md) | [adc-f1](adc-f1.md) | Analog to digital converter |
| [gpio-l0](gpio-l0.md) | [gpio-f1](gpio-f1.md) | General Purpose I/O |
| [hal-l0](hal-l0.md) | [hal-f1](hal-f1.md) | Hardware Abstraction Layer |
| - | [pwm-f1](pwm-f1.md) | Pulse Width Modulation |
| [sleep-l0](sleep-l0.md) | - | Low-power sleep utilities |

| Portable | |
| --- | --- |
| [i2c](i2c.md) | Bit-banged I2C communication driver |
| [spi](spi.md) | Bit-banged SPI communication driver |

| Devices | |
| --- | --- |
| [bme280](bme280.md) | BME280 temp/humidity/pressure sensor |
| [rf69](rf69.md) | RFM69 MHz radio for 434/868/915 MHz |
| [ssd1306](ssd1306.md) | OLED driver for 128x64 and 128x32 displays |
| [mcp9808](mcp9808.md) | Cheap I2C temperature sensor |
| [tsl4531](tsl4531.md) | Digital ambient light I2C sensor |

| Utilities | |
| --- | --- |
| [aes](aes.md) | AES-128 encryption (and [decryption](aes-inv.md)) |
| [pid](pid.md) | PID control loop for temperature control |
| [timed](timed.md) | One-shot and periodic timers |
| [varint](varint.md) | Efficient variable-sized integer encoding |
