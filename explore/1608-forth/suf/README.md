Serial USB driver for Forth - see <http://jeelabs.org/2016/06/standalone-usb-firmware/>

### Boards

* **generic** - many boards, see [eBay](http://www.ebay.com/itm/311156408508) - PA12 high enables USB (sort of)
* **hotcbo** - HotMCU Core Board 1xx see [Haoyu](http://www.hotmcu.com/hystm32f1xxcore144-coredev-board-p-2.html?cPath=1_20) - PF10 low enables USB
* **hytiny** - Hy-TinySTM103T, see [Haoyu](http://www.hotmcu.com/stm32f103tb-arm-cortex-m3-development-board-p-222.html?cPath=1_20) - PA0 low enables USB
* **maplemini** - LeafLabs Maple Mini etc, see [eBay](http://www.ebay.com/itm/400569863658) - PB9 pulsed high enables USB
* **olimexino** - LeafLabs Maple and similar, see [Olimex](https://www.olimex.com/Products/Duino/STM32/OLIMEXINO-STM32/) - PC12 low enables USB
* **olip103** - Olimex STM32 P103 board [Olimex](https://www.olimex.com/Products/ARM/ST/STM32-P103/) - PC11 low enables USB
* **port103z** - Port103Z, see [WaveShare](http://www.waveshare.com/wiki/Port103Z) -
  PA3 low enables USB

> Note: `generic` and `hytiny` have now been combined into a single `common`
> image, which checks the board type at run time and adjusts the USB control
> pin accordingly.

The `.hex` and `.bin` files can be used to flash a board from scratch. These
include the
[g6u/board.fs](https://github.com/jeelabs/embello/blob/master/explore/1608-forth/g6u/board.fs)
and
[g6u/core.fs](https://github.com/jeelabs/embello/blob/master/explore/1608-forth/g6u/core.fs)
code, with drivers and libraries - i.e. _batteries included!_

To upload using [Folie](https://github.com/jeelabs/folie) and a
[SerPlus](http://jeelabs.org/article/1649f/), enter the following command:

    !u path/to/usb-common.hex

Or, even simpler, you can upload directly from the last version on GitHub:

    !u https://raw.githubusercontent.com/jeelabs/embello/master/explore/1608-forth/suf/usb-common.hex

Be sure to refer to the actual _raw_ file contents, not the GitHub page itself!

### Builds

The `f-*.fs` files are used to create these images. Here is an example
transcript of loading the `f-common.fs` file into Folie, while connected to an
F103-based Blue Pill:

```
$ folie
Folie v2.11
Select the serial port:
  1: /dev/cu.Bluetooth-Incoming-Port
  2: /dev/cu.usbmodem32212431
  3: /dev/cu.usbmodem3430DC31
  4: /dev/cu.usbmodemC92AED31
4
Enter '!help' for additional help, or ctrl-d to quit.
[connected to /dev/cu.usbmodemC92AED31]
!s f-common.fs
1> f-common.fs 3: Erase block at  00005000  from Flash
1> f-common.fs 4: Erase block at  00005400  from Flash
Erase block at  00005800  from Flash
[...]
Erase block at  0000B800  from Flash
Finished. Reset �Mecrisp-Stellaris RA 2.3.6 for STM32F103 by Matthias Koch
1> f-common.fs 11: Redefine init.  ok.
1> f-common.fs 35: ( usb end: ) 000063D0  ok.
1> f-common.fs 36: Redefine eraseflash.  ok.
5> board.fs 5: ( board start: ) 00006400  ok.
5> board.fs 36: Redefine init.  ok.
5> board.fs 45: 64 KB <g6u> 32212433 ram/flash: 19108 27648 free  ok.
18> core.fs 5: ( core start: ) 00009400  ok.
18> core.fs 14: 64 KB <g6u> 32212433 ram/flash: 16792 17408 free  ok.
1> f-common.fs 41: hexdump
:100000008C030020EF4D0000074800000748000067
:1000100007480000074800000748000000000000F3
[...]
:10BBE0000000000000000000000000000000000055
:10BBF0000000000000000000000000000000000045
:00000001FF
 ok.
```

As you can see, the hex dump is generated as last step in this process and can
be copied manually to the `usb-common.hex` file.  Note that on the next reset,
the serial connection will be dropped and the board will start listening on its
USB interface. To prevent this, enter `$5000 eraseflashfrom` - this will remove
all the above code again.
