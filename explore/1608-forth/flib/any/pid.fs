\ PID controller written in Forth
\ Based on the code presented here:
\ http://brettbeauregard.com/blog/2011/04/improving-the-beginners-pid-introduction/

\ Setup variables for pid control
0,0 2variable kp           \ absolute value
0,0 2variable ki           \ scaled to sampling interval
0,0 2variable kd           \ scaled to sampling interval
0    variable interval     \ sampling interval (in ms)
0    variable out-limit    \ output limit (0 to `out-limit)
0    variable out-override \ output override (auto mode if -1)

\ Working variables while pid is running
0    variable set-val      \ current setpoint
0,0 2variable total-i      \ cummulative i error
0    variable last-input   \ last seen input

\ =============================================================================
\ Utility words

: f2s ( f -- s )  \ Fixed point to signed number (rounded)
  swap 1 31 lshift and if 1 + then ;

: s2f ( s -- f )  \ Signed number to fixed point
  0 swap ;

: dmin ( d1 d2 -- d_min )  \ Minimum of double number (also for fixed-point)
  2over 2over ( d1 d2 d1 d2 )
  d< if 2drop else 2nip then
;

: dmax ( d1 d2 -- d_max )  \ Maximum of double number (also for fixed-point)
  2over 2over ( d1 d2 d1 d2 )
  d> if 2drop else 2nip then
;

: drange ( d_val d_min d_max -- d_val )  \ Make sure a double number is in range
  2rot ( d_min d_max d_val) dmin dmax
;

: range ( s_val s_min s_max -- s_val )  \ Make sure a number is in range
  rot ( s_min s_max s_val) min max
;

: f.000 3 f.n ;  \ Output fixed point value

\ =============================================================================
\ Main PID - internal definitions (do not call manually)

: calc-p ( f_error -- f_correction )  \ Calculate proportial error
  kp 2@ f*                 \ fetch k-value and scale error
  ." Pval:" 2dup f2s . ;


: calc-i ( f_error -- f_correction )  \ Calculate integral error
  ki 2@ f*                 \ apply ki factor
  total-i 2@ d+            \ sum up with running integral error
  0,0 out-limit @ s2f drange \ cap inside output range
  2dup total-i 2!          \ update running integral error
  ." Ival:" 2dup f2s . ;

: calc-d ( s_is -- f_correction )  \ Calculate differential error
  \ actually use "derivative on input", not on error
  last-input @ -           \ substract last input from current input
  s2f kd 2@ f*             \ make fixed point, fetch k-value and multiply
  ." Dval:" 2dup f2s . ;

: pid_compute ( s_is -- s_corr )  \ Do a PID calculation, return duty-cycle
  cr ." SET:" set-val @ .  ." IS:"  dup . \ DEBUG

  \ feed error in p and i, current setpoint in d, sum up results
  dup dup set-val @ swap - s2f ( s_is s_is f_error )
  2dup  calc-p             ( s_is s_is f_error f_p )
  2swap calc-i d+          ( s_is s_is f_pi )
  rot   calc-d d-          ( s_is f_pid ) \ substract! derivate on input - not error

  f2s                      ( s_is s_corr )
  ." OUT:" dup .           \ DEBUG

  swap last-input !        \ Update variables for next run
  0 out-limit @ range      \ Make sure we return something inside range

  ." PWM:" dup . ;

\ =============================================================================
\ Main PID - external interface

: set ( s -- )  \ Change setpoint on a running pid
  set-val ! ;

: tuning  ( f_kp f_ki f_kd -- )  \ Change tuning-parameters on a running pid
  \ depends on sampletime, so fetch it, move to fixed-point and change unit to seconds
  \ store on return stack for now
  interval @ s2f 1000,0 f/ 2>r

  2r@ f/ kd 2!             \ translate from 1/s to the sampletime
  2r> f* ki 2!             \ translate from 1/s to the sampletime
         kp 2! ;

\ Init PID
\ To use in a *reverse acting system* (bigger output value **reduced**
\ input value make sure `kp`, `ki` and `kd` are **all** negative.
\ Starts pid in manual mode (no setpoint set!). Set setpoint and call auto
\ to start the control loop.
: pid-init ( f_kp f_ki f_kd s_sampletime s_outlimit -- )
  out-limit !
  interval !
  tuning
  0 out-override !         \ Make sure we're in manual mode
  cr ." PID initialized - kp:" kp 2@ f.000 ." ki:" ki 2@ f.000 ." kd:" kd 2@ f.000
;

\ Returns calculated PID value or override value if in manual mode
: pid ( s_is -- s_corr )
  out-override @ -1 = if   \ we're in auto-mode - do PID calculation
    pid_compute
  else                     \ manual-mode! store input, return override value
    cr ." SET:" set-val @ .  ." IS:"  dup .
    last-input !
    out-override @
    ." PWM:" dup .
  then ;

: manual ( s -- )  \ Override output - switches PID into *manual mode*
  out-override ! ;


: auto ( -- )  \ Switch back to auto-mode after manual modex
  out-override @ -1 <> if \ only do something if we'r in override mode
    \ store current output value as i to let it run smoothly
    out-override @
    0 out-limit @ range    \ Make sure we store something within PWM bounds
    s2f total-i 2!
    -1 out-override !
  then ;

: autohold ( -- )  \ Bring PID back to auto-mode after a manual override
  last-input @ set-val !   \ Use last input as setpoint (no bumps!)
  auto ;
