\ Hardware I2C driver for STM32F103.

\ Define pins
[ifndef] SCL  PB6 constant SCL  [then]
[ifndef] SDA  PB7 constant SDA  [then]

$40005400 constant I2C1
$40005800 constant I2C2
     I2C1 $00 + constant I2C1-CR1
     I2C1 $04 + constant I2C1-CR2
     I2C1 $08 + constant I2C1-OAR1
     I2C1 $0C + constant I2C1-OAR2
     I2C1 $10 + constant I2C1-DR
     I2C1 $14 + constant I2C1-SR1
     I2C1 $18 + constant I2C1-SR2
     I2C1 $1C + constant I2C1-CCR
     I2C1 $20 + constant I2C1-TRISE

3  bit constant APB2-GPIOB-EN
0  bit constant APB2-AFIO-EN
21 bit constant APB1-I2C1-EN
21 bit constant APB1-RST-I2C1

\ $40021000 constant RCC
\    RCC $00 + constant RCC-CR
\    RCC $04 + constant RCC-CFGR
     RCC $10 + constant RCC-APB1RSTR
     RCC $14 + constant RCC-AHBENR
\    RCC $18 + constant RCC-APB2ENR
\    RCC $1C + constant RCC-APB1ENR

     0 variable i2c.cnt
     0 variable i2c.addr
     0 variable i2c.needstop
 $ffff variable i2c.timeout

\ Checks I2C1 busy bit
: i2c-busy?   ( -- b) I2C1-SR2 h@ 1 bit and 0<> ;

\ Init and reset I2C. Probably overkill. TODO simplify
: i2c-init ( -- )
  \ Reset I2C1
  APB1-RST-I2C1 RCC-APB1RSTR bis!
  APB1-RST-I2C1 RCC-APB1RSTR bic!

  \ init clocks
  APB2-GPIOB-EN APB2-AFIO-EN or RCC-APB2ENR bis!
  APB1-I2C1-EN                  RCC-APB1ENR bis!

  \ init GPIO
  IMODE-FLOAT SCL io-mode!  \ edited: manual says use floating input
  IMODE-FLOAT SDA io-mode!
  OMODE-AF-OD OMODE-FAST + SCL io-mode!  \ I²C requires external pullup
  OMODE-AF-OD OMODE-FAST + SDA io-mode!  \     resistors on SCL and SDA

  \ Reset I2C peripheral
   15 bit I2C1-CR1 hbis!
   15 bit I2C1-CR1 hbic!

  \ Enable I2C peripheral
  21 bit RCC-APB1ENR bis!  \ set I2C1EN
  $3F I2C1-CR2 hbic!       \ CLEAR FREQ field
  36 I2C1-CR2 hbis!        \ APB1 is 36 MHz

  \ clock rate clock-hz; USB = 72 MHz
  \ crystal 8mhz x 9
  \ PLL 72 MHz / 2
  \ 36 MHz AHB /6
  \ 6 MHz ADC
  \ 001D840A RCC-CFGR
  \ SW: 10    PLL as clock
  \ SWS: 10   PLL as clock
  \ HPRE: 0   AHB = SYSCLK
  \ PPRE1 100 APB1/PCLK1 = /2 must not be > 36 MHz
  \ PPRE2 000 APB2/PCLK2 = /1
  \ ADCPRE 10 ADCPRE = /6
  \ PLLSRC 1  HSE = PLL input
  \ PLLXTPRE 0 HSE /1
  \ PLLMUL 0111 PLL = cryst x9

  \ cryst 8MHz x9 = PLLCLK 72 MHZ
  \ SYSCLK = PLLCLK = 72 MHz
  \ SYSCLK / AHB prescale = 72MHz AHBCLK
  \ AHBCLK / APB1 = 36MHz
  \ AHBCLK / APB2 = 72MHz
  \ I2C = APB1 = 36MHz

  \ Configure clock control registers?!

  27           \ CCR 27?
  15 bit or    \ FM
  I2C1-CCR h!  \ FREQ = 36MHZ, 31 ns; DUTY=0 3x 31ns = 10 MHz; must divide by 27
  3  I2C1-TRISE h!         \ 2+1 for 1000ns SCL

  0  bit I2C1-CR1 hbis!    \ Enable bit
  10 bit I2C1-CR1 hbis!    \ ACK enable

  \ Wait for bus to initialize
  i2c.timeout @ begin 1- dup 0= i2c-busy? 0= or until drop
;

\ debugging
: i2c? cr I2C1-CR1 h@ hex. I2C1-CR2 h@ hex. I2C1-SR1 h@ hex. I2C1-SR2 h@ hex. ;

\ Low level register setting and checking
: i2c-DR!     ( c -- )  I2C1-DR c! ;            \ Writes data register
: i2c-DR@     (  -- c ) I2C1-DR c@ ;            \ Writes data register
: i2c-start!  ( -- )    8 bit I2C1-CR1 hbis! ;
: i2c-stop!   ( -- )    9 bit I2C1-CR1 hbis! ;
: i2c-AF-0 ( -- )  10 bit I2C1-SR1 hbic! ;      \ Clears AF flag
: i2c-START-0 ( -- )   8 bit I2C1-CR1 hbic! ;   \ Clears START condition
: i2c-SR1-flag? ( u -- ) I2C1-SR1 hbit@ ;
: i2c-SR2-flag? ( u -- ) I2C1-SR2 hbit@ ;
: i2c-ACK-1 ( -- ) 10 bit I2C1-CR1 hbis! ;
: i2c-ACK-0 ( -- ) 10 bit I2C1-CR1 hbic! ;
: i2c-POS-1 ( -- ) 11 bit I2C1-CR1 hbis! ;
: i2c-POS-0 ( -- ) 11 bit I2C1-CR1 hbic! ;

\ Low level status checking
: i2c-sb?  ( -- b)   0  bit i2c-SR1-flag? ;     \ Gets start bit flag
: i2c-nak? ( -- b)   10 bit i2c-SR1-flag? ;     \ Gets AF bit flag
: i2c-TxE? ( -- b)   7  bit i2c-SR1-flag? ;     \ TX register empty
: i2c-ADDR? ( -- b)  1  bit i2c-SR1-flag? ;     \ ADDR bit
: i2c-MSL? ( -- b)   0  bit I2C1-SR2 hbit@ ;    \ MSL bit

: i2c-SR1-wait ( u -- ) i2c.timeout @ begin 1- 2dup 0= swap i2c-SR1-flag? or until 2drop ; \ Waits until SR1 meets bit mask or timeout
: i2c-SR1-!wait ( u -- ) i2c.timeout @ begin 1- 2dup 0= swap i2c-SR1-flag? 0= or until 2drop ; \ Waits until SR1 has zero on bit mask or timeout
: i2c-SR2-wait ( u -- ) i2c.timeout @ begin 1- 2dup 0= swap i2c-SR2-flag? or until 2drop ; \ Waits until SR2 meets bit mask or timeout
: i2c-SR2-!wait ( u -- ) i2c.timeout @ begin 1- 2dup 0= swap i2c-SR2-flag? 0= or until 2drop ; \ Waits until SR2 has zero on bit mask or timeout

0  bit constant i2c-SR1-SB
1  bit constant i2c-SR1-ADDR
2  bit constant i2c-SR1-BTF
6  bit constant i2c-SR1-RxNE
7  bit constant i2c-SR1-TxE
10 bit constant i2c-SR1-AF

 0 bit constant i2c-SR2-MSL

\ Medium level actions, no or limited status checking

: i2c-start ( -- ) \ set start bit and wait for start condition
  i2c-start! i2c-SR1-SB i2c-SR1-wait ;

: i2c-stop  ( -- )  i2c-stop! i2c-SR2-MSL i2c-SR2-!wait ; \ stop and wait

: i2c-probe ( c -- nak ) \ Sets address and waits for ACK or NAK
  i2c-start
  shl i2c-DR! \ Send address (low bit zero)
  i2c-SR1-AF i2c-SR1-ADDR or i2c-SR1-wait \ Wait for address sent
  i2c-nak?    \ Put AE on stack (NAK)
  i2c-AF-0
  i2c-stop
;

\ STM32 EV Events

: i2c-EV5   i2c-SR1-SB   i2c-SR1-wait ;
: i2c-EV6a i2c-SR1-ADDR i2c-SR1-AF or i2c-SR1-wait ; \ performs the wait, does not clear ADDR
: i2c-EV6b I2C1-SR1 h@ drop I2C1-SR2 h@ drop ;       \ clears ADDR
: i2c-EV6 i2c-EV6a i2c-EV6b ;                        \ Performs full EV6 action
: i2c-EV8_1 i2c-SR1-TxE  i2c-SR1-wait ;
: i2c-EV7   i2c-SR1-RxNE i2c-SR1-wait ;
: i2c-EV7_2 i2c-SR1-BTF  i2c-SR1-wait ;
: i2c-EV8_2 i2c-EV8_1 i2c-EV7_2 ;                    \ Empty outgoing data

\ Compatibility layer

: i2c-addr ( u --) \ Start a new transaction and send address in write mode
  i2c-start
  shl dup i2c.addr !
  i2c-EV5

  i2c-DR!                   \ Sends address (write mode)
  i2c-EV6a                  \ wait for completion of addressing or AF
;

: i2c-xfer ( u -- nak ) \ prepares for reading an nbyte reply.
                        \ Use after i2c-addr. Stops i2c after completion.
  dup i2c.cnt !
  i2c-EV6b
    case
      2 of    \ cnt = 2
        i2c-start  \ set start bit,  wait for start condition

        i2c.addr @ 1 or \ Send address with read bit
        i2c-DR!

        i2c-POS-1 i2c-ACK-1

        i2c-EV6                  \ wait for ADDR and clear
        i2c-ACK-0
        i2c-SR1-BTF i2c-SR1-wait \ wait for BTF
        i2c-nak?
        i2c-stop!                \ set stop without waiting
        0 i2c.needstop !
      endof
      1 of                      ( cnt = 1 )
        i2c-start                  \ set start bit,  wait for start condition
        i2c.addr @ 1 or            \ Send address with read bit
        i2c-DR!
        i2c-POS-1 i2c-ACK-1
        i2c-EV6a                   \ Wait for addr, do not clear yet
        i2c-ACK-0                  \ Disable ACK
        i2c-EV6b                   \ Clear ADDR
        i2c-nak?
        i2c-stop!                  \ Trigger a stop
        0 i2c.needstop !
      endof
      0 of                      ( cnt = 0, probe only )
	i2c-EV8_2                  \ Flush outbound data first
        i2c-nak? i2c-AF-0          \ push nak flag & clear it
	i2c-stop
        0 i2c.needstop !	
      endof
      ( default: n > 2 )
        i2c-start  \ set start bit,  wait for start condition

        i2c.addr @ 1 or \ Send address with read bit
        i2c-DR!
        i2c-EV6    \ wait until ready to read
        \ i2c-SR1-ADDR i2c-SR1-wait
        i2c-nak?
        1 i2c.needstop !
    endcase
;

: >i2c  ( u -- ) \ Sends a byte over i2c. Use after i2c-addr
  i2c-EV6b                \ Just in case the ADDR needs clearing
  i2c-EV8_1
  i2c-DR!
;

: i2c>

  i2c.needstop @
  if    \ need to do stop stuff when i2c-xfer could not
    i2c.cnt @
    case
      3 of                      ( prepare for last bytes )
        i2c-EV7_2
        i2c-ACK-0
        i2c-DR@
        -1 i2c.cnt +!
        i2c-stop!
      endof
      2 of
        i2c-DR@
        -1 i2c.cnt +!
        0 i2c.needstop !
        \ no further special handling needed
        \ Last byte follows normal protocol
      endof                     ( default action cnt > 3, simple receive )
      i2c-EV7    \ wait until data received
      i2c-DR@
      -1 i2c.cnt +!
    endcase

  else  \ stop stuff was handled in i2c-xfer
    i2c-EV7    \ wait until data received
    i2c-DR@
    -1 i2c.cnt +!
  then

  i2c.cnt @ 0=
  if
    i2c-POS-0 i2c-ACK-1
    i2c-DR@ drop                ( Do not understand why I need this )
  then
;

: i2c>h
    i2c>   i2c>  8 lshift or
;

: i2c>h_inv
    i2c>  8 lshift i2c>  or
;

\ High level transactions

: i2c. ( -- )  \ scan and report all I2C devices on the bus
  128 0 do
    cr i h.2 ." :"
    16 0 do  space i j +
      dup $08 < over $77 > or if drop 2 spaces else
        dup i2c-probe if drop ." --" else h.2 then
      then
    loop
  16 +loop ;
