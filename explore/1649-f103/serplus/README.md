**SerPlus** is an STM32F103-based bridge between USB and a serial port.

Inline telnet escape codes can be used to control DTR, RTS, and parity.

Based on the `usart1_irq_printf` and `usb_cdcacm` code from [libopencm3-examples][EX] (LGPL3).

For the RAM-based version, to be loaded at $20002000, use:

    make -f Makefile-ram

   [EX]: https://github.com/libopencm3/libopencm3-examples/tree/master/examples/stm32/f1/stm32-h103
