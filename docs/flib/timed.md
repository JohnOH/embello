# Callback Timer Library

[code]: any/timed.fs (multi)
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/timed.fs">any/timed.fs</a>
* Needs: multi

This library implements callback style timers using the Mecrisp multitasking module. Every task in the multitasker needs its own stacks which consumes a lot of memory. Using `timed` you can register multiple callbacks in the same timer task and reduce the memory overhead significantly.

The downside is that all the timers share the same *task*. So if one of your *callback words* needs a long time to execute all the other callbacks have to wait for it to complete.

## API
A number identifies each timer (slot). You have to supply the number when registering a timer. If you write a new timer to an already used timer slot, the old one will be overwritten. By default there are 8 timers (slot `0` to `7`) available. Set a constant named `MAX-TIMED` to a different number *before* loading `timed.fs` to change the number of available timer slots.

```
: timed-init ( -- ) \ Init timed module, start

\ Register a callback or cancel a timer
: call-after ( callback when     slot# -- )
: call-every ( callback interval slot# -- )
: call-never ( slot# -- )

: timed. ( -- ) \ Show timers
```

`timed-init` is used to initialize the timed library. It registers the timed background task and starts multitasking.

The second group of words is used to register a *one-shot timer* or a *repeating timer*. `call-never` is used to stop a running timer. A repeating timer will be called as soon as `interval` ms have passed and it repeatedly calls the *callback word* every `interval` ms until it is stopped. `call-after` calls the *callback words* only once as soon as `when` ms have passed.

`timed.` is used to output the current configuration to the console. It shows the running timers, the addresses of the *callback word* and the uptime (`millis`) when the timers executed the last time.


## Examples

Make sure you always initialize timed before using it.

    timed-init

Now we need a *word* to register in the timer. This simple one will do:

    : ping ( -- ) cr ." PING" cr ;

Writing to the console is not recommended as it messes up the console. Nevertheless, it will do for this example.

To execute `ping` in 5 seconds we need to push its address and the other parameters to the stack. We use timer 0 here:

    ' ping 5000 0 call-after

Now wait a few seconds - you will receive `PING` on the console in about 5 seconds. Timed uses the uptime in milliseconds to call the callback at the specific time. Please note that Mecrisp has a cooperative multitasker. So as long as another task is not playing nicely `ping` may be called late. The interactive console works nicely with the multitasker.

Registering a repeating timer is almost the same. This time we call `ping` every 10 seconds and use timer 1:

    ' ping 10000 1 call-every

You can always use `timed.` to show the current state of the timers.

```
timed. 
Slot #0 Interval: 5000 Last-Run: 28345 Callback: 0 Repeat: 0 
Slot #1 Interval: 10000 Last-Run: 49610 Callback: 536871832 Repeat: -1 
Slot #2 Interval: 0 Last-Run: 0 Callback: 0 Repeat: 0 
Slot #3 Interval: 0 Last-Run: 0 Callback: 0 Repeat: 0 
Slot #4 Interval: 0 Last-Run: 0 Callback: 0 Repeat: 0 
Slot #5 Interval: 0 Last-Run: 0 Callback: 0 Repeat: 0 
Slot #6 Interval: 0 Last-Run: 0 Callback: 0 Repeat: 0 
Slot #7 Interval: 0 Last-Run: 0 Callback: 0 Repeat: 0 
 ok.
```

Finally stop the repeating timer:

    1 call-never
