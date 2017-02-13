Serial USB driver for Forth - see <http://jeelabs.org/2016/06/standalone-usb-firmware/>

### Boards

* **generic** - many boards, for example on [eBay](http://www.ebay.com/itm/311156408508) - PA12 high enables USB (sort of)
* **hytiny** - Hy-TinySTM103T, see [Haoyu](http://www.hotmcu.com/stm32f103tb-arm-cortex-m3-development-board-p-222.html?cPath=1_20) - PA0 low enables USB
* **maplemini** - LeafLabs Maple Mini etc, for example on [eBay](http://www.ebay.com/itm/400569863658) - PB9 pulsed high enables USB
* **olimexino** - LeafLabs Maple and similar, for example [Olimex](https://www.olimex.com/Products/Duino/STM32/OLIMEXINO-STM32/) - PC12 low enables USB
* **port103z** - Port103Z, see [WaveShare](http://www.waveshare.com/wiki/Port103Z) -
  PA3 low enables USB

> Note: `generic` and `hytiny` have now been combined into a single `common` image, which determines the board type at run time, and inits the USB re-enumeration pin logic accordingly.
