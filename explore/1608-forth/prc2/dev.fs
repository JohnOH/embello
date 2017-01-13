forgetram

\ up to four input buttons

PB15 constant BTN1
PB14 constant BTN2
PB13 constant BTN3
PB12 constant BTN4
PA8  constant BTN-COMMON

' nop variable K1.handler
' nop variable K2.handler
' nop variable K3.handler
' nop variable K4.handler

PB11 constant LED3
PB10 constant LED4
PB1  constant LED5
PB0  constant LED6

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

  lcd-init show-logo
;

app-init
