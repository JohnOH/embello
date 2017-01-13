forgetram

3400 constant VCC  \ actual Vcc value, in millivolt

: adc ( pin -- value ) dup adc drop adc ; \ read twice, ignore first reading

\ up to four input buttons
PB15 constant BTN1  \ pressed = "1", open = "0"
PB14 constant BTN2  \ pressed = "1", open = "0"
PB13 constant BTN3  \ pressed = "1", open = "0"
PB12 constant BTN4  \ pressed = "1", open = "0"
PA8  constant BTN-COMMON

\ up to four status LEDs
PB11 constant LED3  \ on = "1", off = "0"
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

: pt100-mv ( - u )  \ measure PT100 sensor voltage
  VTEMP adc VCC 4095 */ ;
: heater-mv ( - u )  \ measure heater voltage, almost power rail when on
  VHEATER adc 11 * VCC 4095 */ ;
: usb-mv ( - u )  \ measure USB power rail voltage, 5V when attached to USB
  VUSB adc 2* VCC 4095 */ ;
: power-mv ( - u )  \ measure input power rail voltage, normally 12..24V
  VPOWER adc 11 * VCC 4095 */ ;

: app-init
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

  adc-init
  lcd-init show-logo
;

app-init

(  pt100-mv: ) pt100-mv .
( heater-mv: ) heater-mv .
(    usb-mv: ) usb-mv .
(  power-mv: ) power-mv .
