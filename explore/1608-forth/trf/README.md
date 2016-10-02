The "Tiny RF" node combines an STM32F103 board from [eBay][SB] with an RFM69.  
It uses the "suf" USB-serial driver to connect to a host.

  [SB]: http://www.ebay.com/itm/311156408508

![](image1.jpg)

![](image2.jpg)

### Pin allocation

```
PA4     SSEL
PA5     SCLK
PA6     MISO
PA7     MOSI

PB0     DIO3
PB1     RESET

PA15    DIO5
PB3     DIO1
PB4     DIO2
PB5     DIO0

PC13    LED
```

### Example of use

```
$ folie -p /dev/cu.usbmodem32212431
Connected to: /dev/cu.usbmodem32212431
\       >>> include app.fs
\       >>> include ../mlib/hexdump.fs
\       <<<<<<<<<<< ../mlib/hexdump.fs (75 lines)
\       >>> include ../flib/io-stm32f1.fs
: bit ( u -- u )  \ turn a bit position into a single-bit mask Redefine bit.  ok.
\       <<<<<<<<<<< ../flib/io-stm32f1.fs (69 lines)
\       >>> include ../flib/spi-stm32f1.fs
\       <<<<<<<<<<< ../flib/spi-stm32f1.fs (68 lines)
\       >>> include ../flib/rf69.fs
\       <<<<<<<<<<< ../flib/rf69.fs (183 lines)
\       >>> include ../mlib/multi.fs
\       <<<<<<<<<<< ../mlib/multi.fs (206 lines)
rf69-listen
RF69 21EE068A030052C00107 8101B737A88080
\       <<<<<<<<<<< app.fs (32 lines)
\ done.
RF69 21EE068603005AC00104 81808080
RF69 21EE0687030060C00107 8101B837A98080
RF69 21EE068703005CC00104 81808080
RF69 21EE0687030068C00107 8101B837AA8080
RF69 21EE068803006CC00104 81808080
```
