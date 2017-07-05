\ Virtual disk interface for the eZ80.

include pokemon.fs

PA1  constant LED
PB10 constant BUSY

$E000E100 constant NVIC-EN0R \ IRQ 0 to 31 Set Enable Register

AFIO $08 + constant AFIO-EXTICR1
AFIO $0C + constant AFIO-EXTICR2
AFIO $10 + constant AFIO-EXTICR3
AFIO $14 + constant AFIO-EXTICR4

$40010400 constant EXTI
    EXTI $00 + constant EXTI-IMR
    EXTI $08 + constant EXTI-RTSR
    EXTI $0C + constant EXTI-FTSR
    EXTI $14 + constant EXTI-PR

\ $40020000 constant DMA1
\   DMA1 $00 + constant DMA1-ISR
\   DMA1 $04 + constant DMA1-IFCR
    DMA1 $44 + constant DMA1-CCR4
    DMA1 $48 + constant DMA1-CNDTR4
    DMA1 $4C + constant DMA1-CPAR4
    DMA1 $50 + constant DMA1-CMAR4
    DMA1 $58 + constant DMA1-CCR5
    DMA1 $5C + constant DMA1-CNDTR5
    DMA1 $60 + constant DMA1-CPAR5
    DMA1 $64 + constant DMA1-CMAR5

516 buffer: vreqbuf
0 variable vstatus

: led-on LED ioc! ;
: led-off LED ios! ;
: led-setup  OMODE-PP LED io-mode!  led-off ;

: dma-setup  \ set up the DMA controller channels for SPI2 RX and TX
  0 bit RCC-AHBENR bis!  \ DMA1EN clock enable

  \ DMA1 channel 4: receive from SPI2 RX
  vreqbuf DMA1-CMAR4 !   \ write to eZ80 request buffer
  SPI2-DR DMA1-CPAR4 !   \ read from SPI2
  %10000000 DMA1-CCR4 !  \ MINC

  \ DMA1 channel 5: send to SPI2 TX
  SPI2-DR DMA1-CPAR5 !   \ write to SPI2
  %10010000 DMA1-CCR5 !  \ MINC & DIR
;

: spi2-setup  \ set up I/O pins and SPI2 for slave mode with DMA in and out
  IMODE-PULL ssel2 @ io-mode! -spi2
  IMODE-FLOAT SCLK2 io-mode!
  OMODE-AF-PP MISO2 io-mode!
  IMODE-PULL MOSI2 io-mode!  MOSI2 ioc!
  14 bit RCC-APB1ENR bis!  \ set SPI2EN
  %11 SPI2-CR2 !  \ enable TX and RX DMA
;

: disk-map ( n -- )  \ change specified drive to a new file mapping
  vreqbuf 1+ fat-find swap fat-chain  \ lookup the name and adjust map
  0 vstatus ! vstatus DMA1-CMAR5 !    \ adjust src addr of DMA send channel
;

: disk-rd ( n -- )  \ read sector from file on SD card (128 or 512 bytes)
  vreqbuf @ 8 rshift              \ convert incoming request to offset
  dup 9 rshift                    \ convert offset to block
  rot fat-map sd-read             \ map to file and read the block
  $180 and sd.buf + DMA1-CMAR5 !  \ adjust src addr of DMA send channel
;

: disk-wr ( n -- )  \ write 128-byte sector to file on SD card
  vreqbuf @ 8 rshift              \ convert incoming request to offset
  dup 9 rshift                    \ convert offset to block
  rot fat-map dup sd-read         \ map to file and read the block
  vreqbuf 4 +                     \ address of data to write
  rot $180 and sd.buf + 128 move  \ copy sector into block
  sd-write  0 vstatus !           \ write block and save status
  vstatus DMA1-CMAR5 !            \ adjust src addr of DMA send channel
;

task: disktask

: stop-dma
  0 bit DMA1-CCR4 bic!  \ disable the DMA receive channel
  0 bit DMA1-CCR5 bic!  \ disable the DMA send channel
;

: restart-dma
  516 DMA1-CNDTR4 !  0 bit DMA1-CCR4 bis!  \ restart receives from SPI2 RX
  512 DMA1-CNDTR5 !  0 bit DMA1-CCR5 bis!  \ restart sends to SPI2 TX
;

: reset-spi2  \ disable and re-enable to clear SPI2
  0 SPI2-CR1 !  6 bit SPI2-CR1 ! ;

: disk&  \ this task will process all incoming SPI2 requests for sd card I/O
  disktask background
  begin
    led-on
    stop-dma

    vreqbuf c@ case
      $13 of 0 disk-map endof  \ map D: on file 0
      $14 of 1 disk-map endof  \ map E: on file 1
      $15 of 2 disk-map endof  \ map F: on file 2

      $23 of 0 disk-rd  endof  \ read D: from file 0
      $24 of 1 disk-rd  endof  \ read E: from file 1
      $25 of 2 disk-rd  endof  \ read F: from file 2

      $33 of 0 disk-wr  endof  \ write D: to file 0
    endcase

    reset-spi2
    restart-dma
    BUSY ioc!  \ clear BUSY signal to the eZ80
    led-off
    stop  \ done, suspend, wait for next wake up
  again ;

: zirq-setup  \ set up pin interrupt on rising SPI2 slave select on PB12
  OMODE-PP BUSY io-mode!  BUSY ioc!

  \ set up EXTI interrupt handler to raise BUSY and wake up the disk task
  [: BUSY ios! 12 bit EXTI-PR ! disktask wake ;] irq-exti10 !

     8 bit NVIC-EN1R bis!  \ enable EXTI15_10 interrupt 40
  %0001 AFIO-EXTICR4 bis!  \ select P<B>12
     12 bit EXTI-IMR bis!  \ enable PB<12>
    12 bit EXTI-RTSR bis!  \ trigger on PB<12> rising edge
;

: init-all
  sd-init ." blocks: " sd-size .
  sd-mount. ls
  s" D       IMG" drop fat-find  0 fat-chain  \ build map for D.IMG
  s" E       IMG" drop fat-find  1 fat-chain  \ build map for E.IMG
  s" F       IMG" drop fat-find  2 fat-chain  \ build map for F.IMG
  multitask disk&
  z led-setup zirq-setup dma-setup spi2-setup ;

: ?  ? ." Virtual disk commands:"
  cr ."   vx - launch virtual disk and terminal, hard reset"
  cr ."   vy - launch virtual disk and terminal, soft reset"
  cr ;

: vx init-all x t ;
: vy init-all y t ;
