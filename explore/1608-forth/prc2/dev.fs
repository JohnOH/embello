\ Pico Reflow Controller v2

forgetram

: adc ( pin -- value ) dup adc drop adc ; \ read twice, ignore first reading

: periodic-ms ( handler var ms -- )  \ execute handler every ms milliseconds
  over @ +  millis -  0< if millis swap ! execute else 2drop then ;

\ Constants and variables ------------------------------------------------------

3300 constant VCC  \ actual Vcc value, in millivolt

\ up to four input buttons
PB15 constant BTN1  \ pressed = "1", open = "0"
PB14 constant BTN2  \ pressed = "1", open = "0"
PB13 constant BTN3  \ pressed = "1", open = "0"
PB12 constant BTN4  \ pressed = "1", open = "0"
PA8  constant BTN-COMMON

\ up to four status LEDs
PB11 constant LED3  \ on = "1", off = "0" - heartbeat blinker
PB10 constant LED4  \ on = "1", off = "0"
PB1  constant LED5  \ on = "1", off = "0"
PB0  constant LED6  \ on = "1", off = "0"

\ analog inputs
PA0  constant VTEMP    \ 1k + sens = Vcc
PA1  constant VHEATER  \ 10x + 1k = div 11
PA2  constant VUSB     \ 10k + 10k = div 2
PA3  constant VPOWER   \ 10k + 1k = div 11

\ heater control
PB8  constant HEATER  \ on = "1", off = "0"

\ PT100 sensor calibration values
 21 variable room.temp  260 variable max.temp
270 variable room.mV    530 variable max.mV

\ Analog readings --------------------------------------------------------------

: pt100-mv ( -- u )  \ measure PT100 sensor voltage
  VTEMP adc VCC 4095 */ ;
: heater-mv ( -- u )  \ measure heater voltage, just under power rail when on
  VHEATER adc 11 * VCC 4095 */ ;
: usb-mv ( -- u )  \ measure USB power rail voltage, 5V when attached to USB
  VUSB adc 2* VCC 4095 */ ;
: power-mv ( -- u )  \ measure input power rail voltage, normally 12..24V
  VPOWER adc 11 * VCC 4095 */ ;

: pt100-avg ( -- u )  \ averaged millivolt reading of the PT100
  0  100 0 do pt100-mv + loop  100 / ;
: pt100-deg ( -- u )  \ return PT100 sensor temperature in degrees
\ TODO assumes linear relationship between mV and deg, for now
  pt100-avg room.mV @ -
  max.temp @ room.temp @ -
  max.mV @ room.mV @ - 
  */ room.temp @ + ;

\ Periodic handlers ------------------------------------------------------------

0 variable btn.state  \ one bit for each button, remembering its last state
0 variable btn.timer  \ passed to periodic-ms
0 variable led.timer  \ passed to periodic-ms

: led-blinker [: LED3 iox! ;] led.timer 500 periodic-ms ;

: btn-check-one ( pin bit -- f )  \ true if the button was just pressed
  bit swap ( mask pin )
  io@ 0<> over and swap ( newval mask )
  btn.state @ and ( newval oldval )
  2dup - ?dup if btn.state +! then
  > ;

: button-check
  [: BTN1 0 btn-check-one if ." BTN:1! " then
     BTN2 1 btn-check-one if ." BTN:2! " then
     BTN3 2 btn-check-one if ." BTN:3! " then
     BTN4 3 btn-check-one if ." BTN:4! " then ;]
  btn.timer 100 periodic-ms ;

\ Main application logic -------------------------------------------------------

: app-setup
  lcd-init show-logo
  adc-init

  OMODE-PP   BTN-COMMON io-mode!  BTN-COMMON ios!
  IMODE-PULL BTN1       io-mode!
  IMODE-PULL BTN2       io-mode!
  IMODE-PULL BTN3       io-mode!
  IMODE-PULL BTN4       io-mode!

  OMODE-PP   LED3       io-mode!
  OMODE-PP   LED4       io-mode!
  OMODE-PP   LED5       io-mode!
  OMODE-PP   LED6       io-mode!

  OMODE-PP   HEATER     io-mode!  HEATER ioc!
;

: app-loop
  begin
    led-blinker
    button-check
  key? until ;

\ Go! --------------------------------------------------------------------------

app-setup

(  pt100-mv: ) pt100-mv .
( pt100-deg: ) pt100-deg .
( heater-mv: ) heater-mv .
(    usb-mv: ) usb-mv .
(  power-mv: ) power-mv .

1234 ms app-loop
