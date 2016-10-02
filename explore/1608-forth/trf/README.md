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
\       >>> include ../flib/pins48.fs
\       <<<<<<<<<<< ../flib/pins48.fs (18 lines)
\       >>> include ../flib/spi-stm32f1.fs
\       <<<<<<<<<<< ../flib/spi-stm32f1.fs (68 lines)
\       >>> include ../flib/rf69.fs
\       <<<<<<<<<<< ../flib/rf69.fs (183 lines)
rf69-listen
\       <<<<<<<<<<< app.fs (14 lines)\ done.
RF69 21EE0689030058C00104 81808080
RF69 21EE068803005AC00107 8101B73DA38080
RF69 21EE0689030056C00104 81808080
RF69 21EE068A030068C00107 8101B73DA48080
RF69 21EE068A03005EC00104 81808080
```
