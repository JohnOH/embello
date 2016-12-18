\ explore ADC with the component tester circuit

\ include ../flib/stm32f1/adc.fs

\ fixed pin assignments:
\   PA0, PA1, PA2 = analog inputs (resp: yellow, black, red clips)
\   PB3, PB4, PB5 = 680 Ω, 18 kΩ, 470 kΩ - tied to PA0
\   PB8, PB9, PB10 = 680 Ω, 18 kΩ, 470 kΩ - tied to PA1
\   PB13, PB14, PB15 = 680 Ω, 18 kΩ, 470 kΩ - tied to PA2

\ actual measured / matched resistor values:
673    constant r-low
17700  constant r-mid
466000 constant r-high

\ colour-coded test points:
0 constant yw  \ yellow
1 constant bk  \ black
2 constant rd  \ red

\ configure one pin, where mode is one of:
\   0 = no change
\   1 = set analog mode: floating and schmitt-trigger off
\   2 = set strong high, i.e. "1"
\   3 = set weak pull-up
\   4 = set floating
\   5 = set weak pull-down
\   6 = set strong low i.e. "0"
\ called by "config", returns with both inputs unchanged on the stack
: config-pin ( mode pin -- mode pin )
\ dup io.
  over 7 and case
    1 of            IMODE-ADC             over io-mode!  endof  \ analog
    2 of  dup ios!  OMODE-PP OMODE-FAST + over io-mode!  endof  \ "1"
    3 of  dup ios!  IMODE-PULL            over io-mode!  endof  \ pull-up
    4 of            IMODE-FLOAT           over io-mode!  endof  \ floating
    5 of  dup ioc!  IMODE-PULL            over io-mode!  endof  \ pull-down
    6 of  dup ioc!  OMODE-PP OMODE-FAST + over io-mode!  endof  \ "0"
  endcase ;

\ configure one combined ADC/680/18k/470k test point
\ the argument specifies four pin modes, each in the range 0..6 (see above):
\   $X--- = analog pin
\   $-X-- = digital 680 Ω switch
\   $--X- = digital 18 kΩ switch
\   $---X = digital 470 kΩ switch
\ does some curious "pin arithmetic", knowing that PB14 is PB13+1, i.e.
\   n = 0 : A0/B3/B4/B5, n = 1 : A1/B8/B9/B10, n = 2 : A2/B13/B14/B15
: config ( uuuu n -- )
  dup PA0 + >r 5 * PB5 +        config-pin
  1- ( PB4 ) swap 4 rshift swap config-pin
  1- ( PB3 ) swap 4 rshift swap config-pin
  drop r>    swap 4 rshift swap config-pin  2drop ;

: config-all ( rd bk yw -- )  \ configure all test points at once
  yw config  bk config  rd config ;

: measure ( n -- mv )  \ measure voltage on ADC <n>
  PA0 +  dup adc drop  adc 3300 4095 */ ;
: meas. ( n -- )  \ measure and print voltage with mV accuracy, i.e. 3 decimals
  measure 0 swap  1000,0 f/  0,0005 d+  3 f.n ;
: meas.all ( -- )  cr 25 spaces  rd meas.  bk meas.  yw meas. ;

adc-init
adc-vcc .

: m ( u -- ) dup dup config-all meas.all 10 ms meas.all 10 ms meas.all ;

$1111 m  \ all analog, will vary
$1222 m  \ all tied to Vcc
$1333 m  \ all weak pull-up
$1444 m  \ all floating, will vary
$1555 m  \ all weak pull-down
$1666 m  \ all tied to Gnd

$1111 dup dup config-all  \ end with all analog

: resistor ( hi lo -- val )
  $6666 swap config
  $4244 over config 10 ms dup meas.
  $1424 over config 10 ms dup meas.
  $1442 over config 10 ms dup meas.
  drop
;

bk rd resistor
