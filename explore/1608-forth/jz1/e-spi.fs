\ spi experiment
\ needs core.fs
cr cr reset

\ include ../flib/stm32l0/spi.fs
\ include ../flib/spi/rf69.fs

\ assumes RFM69 is present on PA4..PA7

spi-init spi?

6 rf.group !
\ this causes folie to timeout on include matching, yet still starts running
1234 ms rf-listen
