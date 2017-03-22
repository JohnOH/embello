# PID Control Library

[code]: any/pid.fs ()
* Code: <a href="https://github.com/jeelabs/embello/tree/master/explore/1608-forth/flib/any/pid.fs">any/pid.fs</a>

> A proportional–integral–derivative controller (PID controller) is a control loop feedback mechanism (controller) commonly used in industrial control systems. A PID controller continuously calculates an error value as the difference between a desired setpoint and a measured process variable and applies a correction based on proportional, integral, and derivative terms (sometimes denoted P, I, and D respectively) which give their name to the controller type. (Source: Wikipedia)

This library implements a PID controller. It does not do anything by itself but provides an API to do the calculations. You feed the currently measured variable (*process variable*) in the PID controller and get a correction value (*control value*) back. Where you get the *process variable* from and what you do with the *control value* is your responsibility. You also have to make sure you call `pid` periodically.

You do have to initialize the PID (`pid-init`) before using it for the first time. The PID controller will start in *manual* mode and always return `0` until you set a desired *setpoint* and switch it to `auto` mode.

## API

```
: pid-init ( f_kp f_ki f_kd s_sampletime s_outlimit -- )
: tuning  ( f_kp f_ki f_kd -- ) \ Change tuning parameters on a running PID

: pid ( s_is -- s_corr )        \ Calculate new PID value

: set ( s -- )      \ Change setpoint of a running PID
: manual ( s -- )   \ Manual output override
: auto ( -- )       \ Switch back to auto mode
: autohold ( -- )   \ Switch back to auto mode and hold the current process variable
```

The first words are used to initialize the PID and change the tuning parameters of a running pid controller. All the values for Kp, Ki and Kd are always fixed-point values. Sampletime is in milliseconds and *outlimit* is the maximum value the *control value* (output) will reach. The output will always be a value from 0 to *outlimit* (including).

`pid` is where the magic happens. You are responsible for calling this word periodically with the current *process variable* on the stack, even if the PID controller is currently in *manual* mode. The word `pid` will return the *override value* as long as it is in *manual* mode or the calculated *control value* if it's in *auto* mode.

The last group of commands is for the normal operation. Changing the *setpoint* and switching between *auto* and *manual* mode. Switching to *manual* mode requires a fixed *control value* (output value) which will be returned by `pid` as long as it is running in *manual* mode. Switching back to *auto* mode using `auto` will keep the last supplied setpoint and tries to reach it again. If you use `autohold` to switch back to *auto* mode the PID will use the current process variable as new *setpoint* and tries to hold this value.

## Examples

Initialize and enable PID with Kp=120, Ki=1.5 and Kd=0.0075. 100ms period, maximum output value set to 10'000

    120,0 1,5 0,0075 100 10000 pid-init

You have to implement something which calls `pid` periodically (every 100ms in the example above). It does not matter how you implement this, so this part is omitted here.

Now set initial *setpoint* and start the PID:

```
3100 set \ Set desired setpoint to 3100
auto     \ enable PID
```

Now the controller is running and tries to reach the given *setpoint*.

If you do want to stop the output (set it to `0`) you should use this command:

    0 manual

You do not have to change anything in the PID itself and you still have to call it every 100ms as before. The return value of `pid` will be fixed to the value `0`.

Set a new *setpoint* and enable PID again:

```
2000 set \ Set desired setpoint to 2000
auto     \ enable PID
```
