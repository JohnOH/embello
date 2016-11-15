\ show logo on an OLED attached via I2C

\ these three includes won't fit in ram
<<<core>>>
compiletoflash

include ../flib/i2c-stm32l0.fs
include ../flib/oled.fs
\ include ../mlib/graphics.fs

\ assumes that the OLED is connected to PB6..PB7

\ +i2c i2c? i2c.
lcd-init
show-logo
