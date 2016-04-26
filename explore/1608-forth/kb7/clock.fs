\ adapted from stm32f746g-disco display demo in Mecrisp 2.2.5

8000000 constant HSE_CLK_HZ
16000000 constant HSI_CLK_HZ

: cnt0   ( m -- b )       \ count trailing zeros with hw support
  dup negate and 1-  clz negate 32 + 1-foldable ;
: bits@  ( m adr -- b )   \ get bitfield at masked position after shifting down
  @ over and swap cnt0 rshift ;
: bits!  ( n m adr -- )   \ set bitfield value n to value at masked position
  >R dup >R cnt0 lshift  \ shift value to proper position
  R@ and                 \ mask out unrelated bits
  R> not R@ @ and        \ invert bitmask and maskout new bits in current value
  or r> ! ;              \ apply value and store back

\ $40020000 constant GPIO-BASE

: gpio ( n -- adr )
  $f and 10 lshift GPIO-BASE or 1-foldable ;

$00      constant GPIO_MODER
$04      constant GPIO_OTYPER
$08      constant GPIO_OSPEEDR
$0C      constant GPIO_PUPDR
$10      constant GPIO_IDR
$14      constant GPIO_ODR
$18      constant GPIO_BSRR
$1C      constant GPIO_LCKR
$20      constant GPIO_AFRL
$24      constant GPIO_AFRH

0  GPIO constant GPIOA
1  GPIO constant GPIOB
2  GPIO constant GPIOC
3  GPIO constant GPIOD
4  GPIO constant GPIOE
5  GPIO constant GPIOF
6  GPIO constant GPIOG
7  GPIO constant GPIOH
8  GPIO constant GPIOI
9  GPIO constant GPIOJ
10 GPIO constant GPIOK

: pin#  ( pin -- nr )                    \ get pin number from pin
  $f and 1-foldable ;
: port-base  ( pin -- adr )              \ get port base from pin
  $f bic 1-foldable ;
: port# ( pin -- n )                     \ return gpio port number A:0 .. K:10
  10 rshift $f and 1-foldable ;
: mode-mask  ( pin -- m )
  3 swap pin# 2* lshift 1-foldable ;
: mode-shift ( mode pin -- mode<< ) \ shift mode by pin number*2 for gpio_moder
  pin# 2* lshift 2-foldable ;
: set-mask! ( v m a -- )
  tuck @ swap bic rot or swap ! ;
: bsrr-on  ( pin -- v )                  \ gpio_bsrr mask pin on
  pin# 1 swap lshift 1-foldable ;
: bsrr-off  ( pin -- v )                 \ gpio_bsrr mask pin off
  pin# 16 + 1 swap lshift 1-foldable ;
: af-mask  ( pin -- mask )               \ alternate function bitmask
  $7 and 2 lshift $f swap lshift 1-foldable ;
: af-reg  ( pin -- adr )                 \ alternate function reg addr for pin
  dup $8 and 2/ swap
  port-base GPIO_AFRL + + 1-foldable ;
: af-shift ( af pin -- af )
  pin# 2 lshift swap lshift 2-foldable ;
: gpio-mode! ( mode pin -- )
  tuck mode-shift swap dup
  mode-mask swap port-base set-mask! ;
: mode-af ( af pin -- )
  2 over gpio-mode!
  dup af-mask swap af-reg bits! ;
: speed-mode ( speed pin -- )
  \ set speed mode 0:low speed 1:medium 2:fast 3:high speed
  dup pin# 2* 3 swap lshift
  swap port-base 8 + bits! ;
: mode-af-fast ( af pin -- )
  2 over speed-mode mode-af ;

$40023C00      constant FLASH_ACR

: flash-ws! ( n -- )                     \ set flash latency
  $f FLASH_ACR bits! ;
: flash-prefetch-ena  ( -- )             \ enable prefetch
  1 8 lshift FLASH_ACR bis! ;
: flash-prefetch-dis  ( -- )             \ disable prefetch
  1 8 lshift FLASH_ACR bic! ;
: flash-art-ena?  ( -- f )               \ ART enable ?
  1 9 lshift FLASH_ACR bit@ ;
: flash-art-ena  ( -- )                  \ enable ART
  1 9 lshift FLASH_ACR bis! ;
: flash-art-dis  ( -- )                  \ disable ART
  1 9 lshift FLASH_ACR bic! ;
: flash-art-reset  ( -- )                \ reset ART
  1 11 lshift FLASH_ACR bis! ;
: flash-art-unreset  ( -- )              \ unreset ART
  1 11 lshift FLASH_ACR bic! ;
: flash-art-clear  ( -- )                \ clear art cache
  flash-art-ena?
  flash-art-dis
  flash-art-reset
  flash-art-unreset
  if flash-art-ena then ;

$40023800      constant RCC_BASE         \ RCC base address
$00 RCC_BASE + constant RCC_CR           \ RCC clock control register
$1 18 lshift  constant RCC_CR_HSEBYP    \ HSE clock bypass
$1 17 lshift  constant RCC_CR_HSERDY    \ HSE clock ready flag
$1 16 lshift  constant RCC_CR_HSEON     \ HSE clock enable
$1  1 lshift  constant RCC_CR_HSIRDY    \ Internal high-speed clock ready flag
$1             constant RCC_CR_HSION     \ Internal high-speed clock enable
$04 RCC_BASE + constant RCC_PLLCFGR      \ RCC PLL configuration register
$08 RCC_BASE + constant RCC_CFGR         \ RCC clock configuration register
$20 RCC_BASE + constant RCC_APB1RSTR     \ RCC APB1 peripheral reset register
$30 RCC_BASE + constant RCC_AHB1ENR      \ AHB1 peripheral clock register
$40 RCC_BASE + constant RCC_APB1ENR      \ RCC APB1 peripheral clock enable reg
$44 RCC_BASE + constant RCC_APB2ENR      \ APB2 peripheral clock enable register
$88 RCC_BASE + constant RCC_PLLSAICFGR   \ RCC SAI PLL configuration register
$8C RCC_BASE + constant RCC_DKCFGR1      \ RCC dedicated clocks config register
$90 RCC_BASE + constant RCC_DKCFGR2      \ RCC dedicated clocks config register

$0 constant PLLP/2
$1 constant PLLP/4
$2 constant PLLP/6
$3 constant PLLP/8

$0 constant PPRE/1
$4 constant PPRE/2
$5 constant PPRE/4
$6 constant PPRE/8
$7 constant PPRE/16

$0 constant HPRE/1
$8 constant HPRE/2
$9 constant HPRE/4
$A constant HPRE/8
$B constant HPRE/16
$C constant HPRE/64
$D constant HPRE/128
$E constant HPRE/256
$F constant HPRE/512

$0 constant PLLSAI-DIVR/2
$1 constant PLLSAI-DIVR/4
$2 constant PLLSAI-DIVR/8
$3 constant PLLSAI-DIVR/16

: rcc-gpio-clk-on  ( n -- )    \ enable single gpio port clock 0:GPIOA..10:GPIOK
 1 swap lshift RCC_AHB1ENR bis! ;
: rcc-gpio-clk-off  ( n -- )   \ disable gpio port n clock 0:GPIOA..10:GPIOK
 1 swap lshift RCC_AHB1ENR bic! ;
: rcc-ltdc-clk-on ( -- )                 \ turn on lcd controller clock
  1 26 lshift RCC_APB2ENR bis! ;
: rcc-ltdc-clk-off  ( -- )               \ tun off lcd controller clock
  1 26 lshift RCC_APB2ENR bic! ;
: hse-on  ( -- )                         \ turn on hsi
  RCC_CR_HSEON RCC_CR bis! ;
: hse-stable?  ( -- f )                  \ hsi running ?
  RCC_CR_HSERDY RCC_CR bit@ ;
: hse-wait-stable  ( -- )                \ turn on hsi wait until stable
  begin hse-on hse-stable? until ;
: hse-off  ( -- )                        \ turn off hse
  RCC_CR_HSEON RCC_CR bic! ;
: hse-byp-on  ( -- )                     \ turn on HSE bypass mode
  RCC_CR_HSEBYP RCC_CR bis! ;
: hse-byp-off  ( -- )                    \ turn off HSE bypass mode
  RCC_CR_HSEBYP RCC_CR bic! ;
: hsi-on  ( -- )                         \ turn on hsi
  RCC_CR_HSION RCC_CR bis! ;
: hsi-stable?  ( -- f )                  \ hsi running ?
  RCC_CR_HSIRDY RCC_CR bit@ ;
: hsi-wait-stable  ( -- )                \ turn on hsi wait until stable
  hsi-on begin hsi-stable? until ;
: clk-source-hsi  ( -- )                 \ set system clock to hsi clock
  RCC_CFGR dup @ $3 bic swap ! ;
: clk-source-hse  ( -- )                 \ set system clock to hse clock
  1 3 RCC_CFGR bits! ;
: clk-source-pll  ( -- )                 \ set system clock to pll clock
  2 3 RCC_CFGR bits! ;
: pll-off  ( -- )                        \ turn off main pll
  1 24 lshift RCC_CR bic! ;
: pll-on  ( -- )                         \ turn on main pll
  1 24 lshift RCC_CR bis! ;
: pll-ready?  ( -- f )                   \ pll stable ?
  1 25 lshift RCC_CR bit@ ;
: pll-wait-stable  ( -- )                \ wait until pll is stable
  begin pll-on pll-ready? until ;
: pll-clk-src-hse  ( -- )                \ set main pll source to hse
  1 22 lshift RCC_PLLCFGR bis! ;
: pll-m!  ( n -- )                       \ set main pll clock pre divider
  $1f RCC_PLLCFGR bits! ;
: pll-m@  ( -- n )                       \ get main pll clock pre divider
  $1f RCC_PLLCFGR bits@ ;
: pll-n!  ( n -- )                       \ set Main PLL (PLL) mul factor
  $1ff 6 lshift RCC_PLLCFGR bits! ;
: pll-n@  ( -- n )                       \ get Main PLL (PLL) mul factor
  $1ff 6 lshift RCC_PLLCFGR bits@ ;
: pll-p!  ( n -- )                       \ set Main PLL (PLL) divider
  3 16 lshift RCC_PLLCFGR bits! ;
: pll-p@  ( n -- )                       \ get Main PLL (PLL) divider
  3 16 lshift RCC_PLLCFGR bits@ ;
: pllsai-off  ( -- )                     \ turn off PLLSAI
  1 28 lshift RCC_CR bic! ;
: pllsai-on  ( -- )                      \ turn on PLLSAI
  1 28 lshift RCC_CR bis! ;
: pllsai-ready?  ( -- f )                \ PLLSAI stable ?
  1 29 lshift RCC_CR bit@ ;
: pllsai-wait-stable  ( -- )             \ wait until PLLSAI is stable
  begin pllsai-on pllsai-ready? until ;
: pllsai-n!  ( n -- )                    \ set PLLSAI clock mul factor
  $1ff 6 lshift RCC_PLLSAICFGR bits! ;
: pllsai-r!  ( n -- )                    \ set PLLSAI clock division factor
  $7 28 lshift RCC_PLLSAICFGR bits! ;
: pllsai-divr!  ( n -- )                 \ division factor for LCD_CLK
  $3 16 lshift RCC_DKCFGR1 bits! ;
: ahb-prescaler! ( n -- )                \ set AHB prescaler
  $F0 RCC_CFGR bits! ;
: apb1-prescaler! ( n -- )               \ set APB1 low speed prescaler
  $7 10 lshift RCC_CFGR bits! ;
: apb2-prescaler! ( n -- )               \ set APB2 high speed prescaler
  $7 13 lshift RCC_CFGR bits! ;

$40007000      constant PWR_BASE         \ PWR base address
$00 PWR_BASE + constant PWR_CR1          \ PWR power control register
$04 PWR_BASE + constant PWR_CSR1         \ PWR power control/status register

: overdrive-enable ( -- )                \ enable over drive mode
  1 16 lshift PWR_CR1 bis! ;
: overdrive-ready? ( -- f )              \ overdrive ready ?
  1 16 lshift PWR_CSR1 bit@ ;
: overdrive-switch-on  ( -- )            \ initiate overdrive switch
  1 17 lshift PWR_CR1 bis! ;
: overdrive-switch-ready?  ( -- f )      \ overdrive switch complete
  1 17 lshift PWR_CSR1 bit@ ;
: pwr-clock-on  ( -- )                   \ turn on power interface clock
  $01 28 lshift RCC_APB1ENR bis! ;
: overdrive-on ( -- )                    \ turn overdrive on
  pwr-clock-on
  overdrive-enable
  begin overdrive-ready? until
  overdrive-switch-on
  begin overdrive-switch-ready? until ;
: voltage-scale-mode-3  ( -- )           \ activate voltage scale mode 3
  1 $03 14 lshift PWR_CR1 bits! ;
: voltage-scale-mode-1  ( -- )           \ activate voltage scale mode 3
  3 $03 14 lshift PWR_CR1 bits! ;

$40011000               constant USART1_BASE
$0C                     constant USART_BRR
USART_BRR USART1_BASE + constant USART1_BRR

: usart1-clk-sel!  ( n -- )              \ set usart1 clk source
  $3 RCC_DKCFGR2 bits! ;
: usart1-baud-update!  ( baud -- )       \ update usart baudrate
  2 usart1-clk-sel!                      \ use hsi clock
  HSI_CLK_HZ over 2/ + swap /            \ calc baudrate for 16x oversampling
  USART1_BRR ! ;

: 16MHz  ( -- )  \ set clock to default speed, using internal RC
  hsi-wait-stable
  clk-source-hsi                         \ switch to hsi clock for reconfig
  pll-off hse-off 0 flash-ws! flash-prefetch-dis
  16000000 clock-hz !
  115200 usart1-baud-update! ;

: 216MHz  ( -- )  \ set clock to maximum speed
  16Mhz                                  \ switch to hsi clock for reconfig
  hse-off hse-byp-off hse-on             \ NO hse bypass mode
  pll-off pll-clk-src-hse                \ pll use hse as clock source
  HSE_CLK_HZ 1000000 / PLL-M!            \ PLL input clock 1 Mhz
  432 pll-n! PLLP/2 PLL-P!               \ VCO clock 400 MHz
  voltage-scale-mode-1                   \ for flash clock > 168 MHz V scale 1
  overdrive-on                           \ for flash clock > 180 overdrive mode
  hse-wait-stable                        \ hse must be stable before use
  pll-on
  flash-prefetch-ena                     \ activate prefetch to reduce latency
  7 flash-ws!
  flash-art-clear                        \ prepare cache
  flash-art-ena                          \ turn on cache
  HPRE/1 ahb-prescaler!                  \ 216 MHz AHB
  PPRE/2 apb2-prescaler!                 \ 108 MHz APB2
  PPRE/4 apb1-prescaler!                 \ 54 MHz APB1
  pll-wait-stable clk-source-pll
  216000000 clock-hz ! ;
