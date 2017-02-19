# Low-power sleep

[code]: stm32l0/sleep.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/stm32l0/sleep.fs">stm32l0/sleep.fs</a>

Provide access to the low-power timer for low-power sleeping (stop mode).

### API

[defs]: <> (lptim-init lptim?)
```
: lptim-init ( -- )  \ enable the low-power timer
: lptim? ( -- )  \ dump the low-power timer registers
```

[defs]: <> (stop100ms stop1s stop10s)
```
: stop100ms ( -- )  \ sleep in low-power for 100 ms
: stop1s    ( -- )  \ sleep in low-power for 1 sec
: stop10s   ( -- )  \ sleep in low-power for 10 sec
```

[defs]: <> (wfe)
```
: wfe ( -- )  \ WFE Opcode, enters sleep mode
```

### Examples

    lptim-init
    stop100ms

```
: lp-blink ( -- )  only-msi  begin  stop1s led iox!  again ;
lptim-init lp-blink
```
