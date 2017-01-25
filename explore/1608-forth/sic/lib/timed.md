# Callback Timer Library

* Code: `timed.fs`
* Needs: `multi.fs`

This library implements callback style timers using the Mexcrisp multitasking module. Every task in the multitasker needs its
own stacks which consumes a lot of memory. Using `timed` you can register multiple callbacks in the same *timer task* and reduce
the memory overhead.

The downside is that all the timers share the same *task*. So if one of your *callback words* needs a long time to execute all
the other timers have to wait for it to complete.

## API
A number identifies the timers. You have to supply the number when registering a timer. If you write a new timer to an already
used timer number, the old one is overwritten. By default there are 8 timer (number `0` to `7`) available. Change the
`max-timed` constant in `timed.fs` if you need more.

```
: timed-init ( -- ) \ Init timed module, start

\ Register a callback or cancel a timer
: call-after ( callback when     timed# -- )
: call-every ( callback interval timed# -- )
: call-never ( timed# -- )

: timed. ( -- ) \ Show timers
```

`timed-init` is used to initialize the timed library. It registers the timed background task and starts multitasking.

The second group of words is used to register a *one-shot timer* or a *repeating timer*. `call-never` is used to stop a
running timer. A repeating timer will be called as soon as `interval` ms have passed and it repeatedly calls the *callback word*
every `interval` ms until it is stopped. `call-after` calls the *callback words* only once as soon as `when` ms have passed.

`timed.` is used to output the current configuration to the console. It shows the running timers, the address of the
*callback word* and the uptime (`millis`) when the specific timers executed the last time.


## Examples

Make sure you always initialize timed before using it.

    timed-init

Now we need a *word* to register in the timer. This simple one will do:

    : ping ( -- ) CR ." PING" CR ;

Writing to the console is not recommended as it messes up the console. Nevertheless, it will do for this example.

To execute `ping` in 5 seconds we need to push its address and the other parameters to the stack. We use timer 0 here:

    ' ping 5000 0 call-after

Now wait a few seconds - you will receive `PING` on the console in about 5 seconds. Timed uses the uptime in milliseconds
to call the callback at the specific time. Please note that Mecrisp has a cooperative multitasker. So as long as another
task is not playing nicely `ping` may be called late.

Registering a repeating timer is almost the same. This time we call `ping` every 10 seconds and use timer 1:

    ' ping 10000 1 call-every

You can always use `timed.` to show the current state of the timers.

Finally stop the repeating timer:

    1 call-never
