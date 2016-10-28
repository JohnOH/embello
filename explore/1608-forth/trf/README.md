The "Tiny RF" node combines an STM32F103 board (like [this one][SB]) with an RFM69CW.  
It uses the "suf" USB-serial driver to connect to a host.

  [SB]: http://www.ebay.com/itm/311156408508

![](image1.jpg)

![](image2.jpg)

![](image3.jpg)

### RFM69CW pin map

```text
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

As entered:

```text
folie           <= start the Folie utility from the command line
2               <= select the second serial port
eraseflash      <= this is a redefined version which preserves the USB driver
...             <= connection is lost, but folie automatically recovers
!s trf69.fs     <= last command entered, radio listening starts automatically

```

Sample output transcript:

```text
$ folie
Folie v2.4-1-g80ccc20
Select the serial port:
  1: /dev/cu.Bluetooth-Incoming-Port
  2: /dev/cu.usbmodemC920C931
? 2
Enter '!help' for additional help, or ctrc-d to quit.
[connected to /dev/cu.usbmodemC920C931]
  ok.
eraseflash
Finished. Reset !
Unha
[disconnected] no such file or directory
[connected to /dev/cu.usbmodemC920C931]
  ok.
!s trf69.fs
>>> io-stm32f1.fs 11: Redefine bit.  ok.
4: rf69-listen
RF69 21EE0600000000C00104 81808080
RF69 21EE067F040036C00107 8101AB29B08080
RF69 21EE067D04004AC00107 8101AA29B18080
 ok.
  ok.
```
