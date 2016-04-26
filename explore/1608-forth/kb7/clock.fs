\ adapted from stm32f746g-disco display demo in Mecrisp 2.2.5

8000000 constant HSE_CLK_HZ
16000000 constant HSI_CLK_HZ

: cnt0 ( m -- b )        \ count trailing zeros with hw support
  dup negate and 1-  clz negate 32 + 1-foldable ;
: bits@ ( m adr -- b )   \ get bitfield at masked position after shifting down
  @ over and swap cnt0 rshift ;
: bits! ( n m adr -- )   \ set bitfield value n to value at masked position
  >R dup >R cnt0 lshift  \ shift value to proper position
  R@ and                 \ mask out unrelated bits
  R> not R@ @ and        \ invert bitmask and maskout new bits in current value
  or r> ! ;              \ apply value and store back

: pin# ( pin -- nr ) \ get pin number from pin
  $f and 1-foldable ;
: port-base ( pin -- adr ) \ get port base from pin
  $f bic 1-foldable ;
: port# ( pin -- n ) \ return gpio port number A:0 .. K:10
  10 rshift $f and 1-foldable ;
: mode-mask ( pin -- m )
  3 swap pin# 2* lshift 1-foldable ;
: mode-shift ( mode pin -- mode<< ) \ shift mode by pin number*2 for gpio_moder
  pin# 2* lshift 2-foldable ;
: set-mask! ( v m a -- )
  tuck @ swap bic rot or swap ! ;
: bsrr-on ( pin -- v ) \ gpio_bsrr mask pin on
  pin# 1 swap lshift 1-foldable ;
: bsrr-off ( pin -- v ) \ gpio_bsrr mask pin off
  pin# 16 + 1 swap lshift 1-foldable ;
: af-mask ( pin -- mask ) \ alternate function bitmask
  $7 and 2 lshift $f swap lshift 1-foldable ;
: af-reg ( pin -- adr ) \ alternate function reg addr for pin
  dup $8 and 2/ swap
  port-base $20 + + 1-foldable ;
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

: flash-ws! ( n -- ) $f FLASH_ACR bits! ;
: flash-prefetch-ena ( -- ) 8 bit FLASH_ACR bis! ;
: flash-prefetch-dis ( -- ) 8 bit FLASH_ACR bic! ;
: flash-art-ena? ( -- f ) 9 bit FLASH_ACR bit@ ;
: flash-art-ena ( -- ) 9 bit FLASH_ACR bis! ;
: flash-art-dis ( -- ) 9 bit FLASH_ACR bic! ;
: flash-art-reset ( -- ) 11 bit FLASH_ACR bis! ;
: flash-art-unreset ( -- ) 11 bit FLASH_ACR bic! ;
: flash-art-clear ( -- )
  flash-art-ena?
  flash-art-dis flash-art-reset flash-art-unreset
  if flash-art-ena then ;

$40023800      constant RCC_BASE         \ RCC base address
$00 RCC_BASE + constant RCC_CR           \ RCC clock control register
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

: rcc-gpio-clk-on ( n -- ) 1 swap lshift RCC_AHB1ENR bis! ;
: rcc-gpio-clk-off ( n -- ) 1 swap lshift RCC_AHB1ENR bic! ;
: rcc-ltdc-clk-on ( -- ) 26 bit RCC_APB2ENR bis! ;
: rcc-ltdc-clk-off ( -- ) 26 bit RCC_APB2ENR bic! ;
: hse-on ( -- ) 16 bit RCC_CR bis! ;
: hse-stable? ( -- f ) 17 bit RCC_CR bit@ ;
: hse-wait-stable ( -- ) begin hse-on hse-stable? until ;
: hse-off ( -- ) 16 bit RCC_CR bic! ;
: hse-byp-on ( -- ) 18 bit RCC_CR bis! ;
: hse-byp-off ( -- ) 18 bit RCC_CR bic! ;
: hsi-on ( -- ) 0 bit RCC_CR bis! ;
: hsi-stable? ( -- f ) 1 bit RCC_CR bit@ ;
: hsi-wait-stable ( -- ) hsi-on begin hsi-stable? until ;
: clk-source-hsi ( -- ) RCC_CFGR dup @ $3 bic swap ! ;
: clk-source-hse ( -- ) 1 3 RCC_CFGR bits! ;
: clk-source-pll ( -- ) 2 3 RCC_CFGR bits! ;
: pll-off ( -- ) 24 bit RCC_CR bic! ;
: pll-on ( -- ) 24 bit RCC_CR bis! ;
: pll-ready? ( -- f ) 25 bit RCC_CR bit@ ;
: pll-wait-stable ( -- ) begin pll-on pll-ready? until ;
: pll-clk-src-hse ( -- ) 22 bit RCC_PLLCFGR bis! ;
: pll-m! ( n -- ) $1f RCC_PLLCFGR bits! ;
: pll-m@ ( -- n ) $1f RCC_PLLCFGR bits@ ;
: pll-n! ( n -- ) $1ff 6 lshift RCC_PLLCFGR bits! ;
: pll-n@ ( -- n ) $1ff 6 lshift RCC_PLLCFGR bits@ ;
: pll-p! ( n -- ) 3 16 lshift RCC_PLLCFGR bits! ;
: pll-p@ ( n -- ) 3 16 lshift RCC_PLLCFGR bits@ ;
: pllsai-off ( -- ) 28 bit RCC_CR bic! ;
: pllsai-on ( -- ) 28 bit RCC_CR bis! ;
: pllsai-ready? ( -- f ) 29 bit RCC_CR bit@ ;
: pllsai-wait-stable ( -- ) begin pllsai-on pllsai-ready? until ;
: pllsai-n! ( n -- ) $1ff 6 lshift RCC_PLLSAICFGR bits! ;
: pllsai-r! ( n -- ) $7 28 lshift RCC_PLLSAICFGR bits! ;
: pllsai-divr! ( n -- ) $3 16 lshift RCC_DKCFGR1 bits! ;
: ahb-prescaler! ( n -- ) $F0 RCC_CFGR bits! ;
: apb1-prescaler! ( n -- ) $7 10 lshift RCC_CFGR bits! ;
: apb2-prescaler! ( n -- ) $7 13 lshift RCC_CFGR bits! ;

$40007000      constant PWR_BASE         \ PWR base address
$00 PWR_BASE + constant PWR_CR1          \ PWR power control register
$04 PWR_BASE + constant PWR_CSR1         \ PWR power control/status register

: overdrive-enable ( -- ) 16 bit PWR_CR1 bis! ;
: overdrive-ready? ( -- f ) 16 bit PWR_CSR1 bit@ ;
: overdrive-switch-on ( -- ) 17 bit PWR_CR1 bis! ;
: overdrive-switch-ready? ( -- f ) 17 bit PWR_CSR1 bit@ ;
: pwr-clock-on ( -- ) 28 bit RCC_APB1ENR bis! ;
: overdrive-on ( -- )
  pwr-clock-on
  overdrive-enable  begin overdrive-ready? until
  overdrive-switch-on  begin overdrive-switch-ready? until ;
: voltage-scale-mode-3 ( -- ) 1 $03 14 lshift PWR_CR1 bits! ;
: voltage-scale-mode-1 ( -- ) 3 $03 14 lshift PWR_CR1 bits! ;

$40011000               constant USART1_BASE
$0C                     constant USART_BRR
USART_BRR USART1_BASE + constant USART1_BRR

: usart1-clk-sel! ( n -- ) \ set usart1 clk source
  $3 RCC_DKCFGR2 bits! ;
: usart1-baud-update! ( baud -- ) \ update usart baudrate
  2 usart1-clk-sel!                      \ use hsi clock
  HSI_CLK_HZ over 2/ + swap /            \ calc baudrate for 16x oversampling
  USART1_BRR ! ;

: 16MHz ( -- ) \ set clock to default speed, using internal RC
  hsi-wait-stable clk-source-hsi
  pll-off hse-off 0 flash-ws! flash-prefetch-dis
  16000000 clock-hz !
  115200 usart1-baud-update! ;

: 216MHz ( -- ) \ set clock to maximum speed
  16Mhz                                  \ switch to hsi clock for reconfig
  hse-byp-off hse-on pll-clk-src-hse
  HSE_CLK_HZ 1000000 / PLL-M!            \ PLL input clock 1 Mhz
  432 pll-n! PLLP/2 PLL-P!               \ VCO clock 432 MHz
  voltage-scale-mode-1 overdrive-on
  hse-wait-stable pll-on
  flash-prefetch-ena 7 flash-ws! flash-art-clear flash-art-ena
  HPRE/1 ahb-prescaler!                  \ 216 MHz AHB
  PPRE/2 apb2-prescaler!                 \ 108 MHz APB2
  PPRE/4 apb1-prescaler!                 \ 54 MHz APB1
  pll-wait-stable clk-source-pll
  216000000 clock-hz ! ;
