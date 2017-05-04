A collection of files for use with Mecrisp Stellaris Forth on STM32 boards. See
[this article](http://jeelabs.org/2016/06/thoughts-about-app-structure/) for the
structure of these directories, and how source files should be loaded:

* **`bct`** - Blue Pill Component Tester, explores the ADC & GPIO pins
* **`bme`** - BME280 sensor exploration using the Olimexino-STM32
* **`cag`** - Console Access Gateway w/ STM32F103, acting as central RF console
* **`eee`** - Experimental Engine Explorations on a HyTiny
* **`g6s`** - Generic 64-pin F103 chips for use with the serial port (USART1)
* **`g6u`** - Generic 64-pin F103 chips for use with USB console driver
* **`i2c`** - I2C test setup with lots of breakout boards and JeeLabs plugs
* **`lnr`** - Led Node Revisited - using a JeeNode Zero to drive LEDs via PWM
* **`prc`** - Pico Reflow Controller w/ HyTiny, OLED, MOSFET, RFM69, 12-24V Vin
* **`qld`** - Quick Loader -  using a Blue Pill to  re-flash a JNZ via SPI
* **`rfc`** - Remote Console driver, routes console I/O over RF
* **`rvm`** - Remote voltmeter w/ STM32L052 and a 4-chan Analog Plug
* **`sic`** - Soldering Iron Controller
* **`suf`** - Serial USB driver for Forth, routes console I/O over USB
* **`ten`** - Test Echo Node, used for testing JeeNode Zero boards
* **`tex`** - Tiny Extender, a HyTiny w/ extender board for RFM69 + SPI flash
* **`trf`** - Tiny RF node, a bridge from RFM69 to USB serial
* **`zeb`** - STM32F103ZE "Basic" board w/ µSD and two 2x32-pin headers

These files implement a range of hardware drivers and other generic functions:

* **`flib`** - Forth library, various modules used by the above boards
* **`flib/mecrisp`** - Mecrisp library, copied / modified from Mecrisp code

Most of the above projects use Mecrisp Forth "core" builds from this area:

* **`cores`** - Matthias Koch's Mecrisp Stellaris Forth with minor extensions

The following older projects use files called `h` (hardware), `l` (library), and
`d` (development), which should be loaded in that order. However, due to changes
elsewhere, it is very likely that they won't work as is anymore, as of 2017.

* **`aia`** - ARMinARM, Raspberry add-on w/ STM32F103RE
* **`cbf`** - Haoyu Core Board Four board w/ STM32F407ZG and lots of RAM+flash
* **`cbo`** - Haoyu Core Board One board w/ STM32F103ZE and lots of RAM+flash
* **`dad`** - Dime-A-Dozen, for all those cheap eBay STM32F103C8 boards
* **`gd4`** - GoldDragon 407 w/ STM32F407ZG and 3.2" LCD
* **`hmv`** - Haoyu Hy-STM32MiniV board w/ STM32F103VC and 3.2" LCD
* **`kb7`** - Ken Boak's STM32F746VG Break-Out-Board
* **`lsd`** - Little Shark Display board w/ STM32F107RC and 1.44" LCD
* **`mrn`** - Multi Receiver Node w/ STM32F103C8 and some wireless modules
* **`oxs`** - Olimexino-STM32 board w/ STM32F103RB, CAN, µSD, and LiPo charger
* **`rnw`** - RF Node Watcher w/ HyTiny STM32F103TB, RFM69, and 128x64 OLED
* **`wpz`** - WaveShare Port103Z w/ STM32F103ZE
* **`ybc`** - Yellow Blue STM32F103VC board
* **`ztw`** - Zero To Wireless demo w/ STM32F103C8 "Blue Pill" and RFM69CW

For more details, see the JeeLabs weblog posts and articles:

* <http://jeelabs.org/2016/02/dive-into-forth/>
* <http://jeelabs.org/2016/03/dive-into-forth-part-2/>
* <http://jeelabs.org/2016/03/dive-into-forth-part-3/>
