# HAL for STM32L0

[code]: stm32l0/hal.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32l0/hal.fs">stm32l0/hal.fs</a>

The Hardware Abstraction Layer for STM32L0 microcontrollers defines
utilities to make code more portable across several architecture
variations.

## API

[defs]: <> (32MHz 16MHz 2.1MHz 65KHz hsi-on hsi-wakeup only-msi)
```
: 32MHz ( -- )  \ set the main clock to 32 MHz, using the PLL
: 16MHz ( -- )  \ set the main clock to 16 MHz (HSI)
: 2.1MHz ( -- )  \ set the main clock to 2.1 MHz (MSI)
: 65KHz ( -- )  \ set main clock to 65 KHz, assuming it was set to 2.1 MHz
: hsi-on ( -- )  \ turn on internal 16 MHz clock, needed by ADC
: hsi-wakeup ( -- )  \ wake up using the 16 MHz clock
: only-msi ( -- )  \ turn off HSI16, this disables the console UART
```

[defs]: <> (systick-hz micros millis us ms)
```
: systick-hz ( u -- )  \ enable systick interrupt at given frequency
: micros ( -- u )  \ return elapsed microseconds, this wraps after some 2000s
: millis ( -- u )  \ return elapsed milliseconds, this wraps after 49 days
: us ( n -- )  \ microsecond delay using a busy loop, this won't switch tasks
: ms ( n -- )  \ millisecond delay, multi-tasker aware (may switch tasks!)
```

[defs]: <> (baud list)
```
: baud ( u -- u )  \ calculate baud rate divider, based on current clock rate
: list ( -- )  \ list all words in dictionary, short form
```

[defs]: <> (chipid hwid flash-kb flash-pagesize)
```
: chipid ( -- u1 u2 u3 3 )  \ unique chip ID as N values on the stack
: hwid ( -- u )  \ a "fairly unique" hardware ID as single 32-bit int
: flash-kb ( -- u )  \ return size of flash memory in KB
: flash-pagesize ( addr - u )  \ return size of flash page at given address
```

## Variables

[defs]: <> (clock-hz)
```
16000000  variable clock-hz  \ the system clock is 16 MHz after reset
```
