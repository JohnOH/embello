\ show logo on an OLED attached via I2C

\ these includes won't all fit in ram
<<<core>>>
compiletoflash

include ../flib/stm32l0/i2c.fs
include ../flib/i2c/oled.fs
\ include ../mlib/graphics.fs

\ assumes that the OLED is connected to PB6..PB7

\ +i2c i2c? i2c.
lcd-init
show-logo
