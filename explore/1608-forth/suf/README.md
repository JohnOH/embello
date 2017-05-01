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
