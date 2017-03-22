<<<core>>>


\ Stops the heating
: stop-heating ( -- )
 0 manual
;

\ Internal temperature representation to real temperature (rounded!)
: int2temp ( s -- s )
  10 /
;

\ Real temperature to internal representation
: temp2int ( s -- s )
  10 *
;

\ Init pwm for output
PB6 constant PWM-OUT
120 PWM-OUT pwm-init
0 PWM-OUT pwm

\ and the MOSFET for protecting the OpAmps input
PB5 constant OPPROT-OUT
OMODE-PP OPPROT-OUT io-mode!

: opamp-prot   ( -- ) OPPROT-OUT ios! ; \ protect OpAmp input by shorting input to Gnd
: opamp-unprot ( -- ) OPPROT-OUT ioc! ; \ disable protection


\ Init adc for input
PB0 constant ADC-IN
adc-init adc-calib
IMODE-ADC ADC-IN io-mode!


\ Decide based on adc input if tip is missing
: notip? ( s_adc -- flag )
  4000 >
;


\ Measures adc input and checks for some common error conditions
\ Switches to manual mode if there is an errror
\ Result is in 1/10K (diff to environment)
: measure ( -- s_temp )
  ADC-IN adc ADC-IN adc + 2/
  dup notip? IF
    ." No tip connected - stopping pid"
    stop-heating
    drop -1
  ELSE
    \ y=0.1098x+29.471 (*5 as we're working with 1/10K)
    s2f 1,098 f* 294,71 d+ f2s
  THEN
;


\ Check if the temp goes up while heating
0 variable heat-start
0 variable heat-temp
0 variable heat-state

: heatmon-checkifok ( pwm -- pwm)
  0 heat-state !
  last-input @ heat-temp @ - 50 < IF
    \ less than 5K? There has to be an error
    ." Heatermonitor decided there is something wrong as temp is NOT rising - stopping"
    ." Temp at start:" heat-temp @ int2temp . ." now:" last-input @ int2temp .
    stop-heating
    drop 0   \ force pwm out to 0
  ELSE
    ." heatmon is happy - stopped"
  THEN
;

: heatmon-ison ( pwm -- pwm )
  millis heat-start @ - 2000 > IF \ at least 2sec passed
    heatmon-checkifok
  ELSE \ check if we're still needed here
    dup 6000 < IF \ pwm < 6000, disable myself
      0 heat-state !
      ." heatmon stopped"
    THEN
  THEN
;

: heatmon-isoff ( pwm -- pwm )
  dup 8000 > IF \ check if PWM > 8000
    1 heat-state !
    millis heat-start !
    last-input @ heat-temp !
    ." heatmon started"
  THEN
;

: heatmon ( pwm -- pwm )
  heat-state @ case
  0 of \ standby or not initialized
    heatmon-isoff
    endof
  1 of \ temp *should* go up
    heatmon-ison
    endof
  endcase
;

: output-is ( -- )
  ." TEMP:"
  last-input @ int2temp .
;


\ Control loop

0 variable next-step
0 variable wait-time

: next ( step wait -- ) 1 - wait-time ! next-step !  ;

: 1mshandler ( -- )
  wait-time @ 0<> IF
    -1 wait-time +!
  ELSE
    next-step @ case
    0 of \ == 0 -> disable pwm
      0 PWM-OUT pwm
      1 2 next \ next step in 2 ms
      endof
    1 of \ == 1 -> unprotect opamp
      opamp-unprot
      2 8 next \ next step in 9 ms
      endof
    2 of \ == 2 -> do the work (measure, protect opamp, calculate pid)
      measure opamp-prot pid heatmon PWM-OUT pwm
      output-is ." °C"
      0 90 next \ next step in 90 ms
      endof
    endcase
  THEN
; 

\ include pinchange.fs

\ add 1mshandler for pid control to systick handler
: ++ticks ( -- ) ++ticks 1mshandler ;

\ enable the new ++ticks implementation 
: enable-systick-pid ( -- )
  1020 ms \ wait a bit for folie to time out
  ['] ++ticks irq-systick !
;


\ Init PID
120,0 1,5 0,0075 100 10000 pid-init
3100 set \ about 330°C @20°C environment
enable-systick-pid
