# HAL for STM32F1

[code]: stm32f1/hal.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32f1/hal.fs">stm32f1/hal.fs</a>

The Hardware Abstraction Layer for STM32F1 microcontrollers defines
utilities to make code more portable across several architecture
variations.

## API

[defs]: <> (72MHz 8MHz)
```
: 72MHz ( -- )  \ set the main clock to 72 MHz, keep baud rate at 115200
: 8MHz ( -- )  \ set the main clock back to 8 MHz, keep baud rate at 115200
```

[defs]: <> (systick-hz micros millis us ms)
```
: systick-hz ( u -- )  \ enable systick interrupt at given frequency
: micros ( -- n )  \ return elapsed microseconds, this wraps after some 2000s
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
8000000 variable clock-hz  \ the system clock is 8 MHz after reset
```
