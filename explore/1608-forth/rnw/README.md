**An exploration of Mecrisp-Stellaris Forth 2.2.1 on STM32F103 µCs.**

This code runs on the "RF Node Watcher" hardware described on the [weblog][R].
The RNW includes an ARM-based [Hy-TinySTM103T][H], an RFM69 connected over SPI,
and a small 128x64 pixel OLED display connected over I2C. It should also work
on many other STM32F103 boards, see the "h" file for pin definitions.

> Note: there's currently no USB support, only serial @ 115200 baud on PA9 (TX)
and PA10 (RX) !

## Installation

The board must have Mecrisp-Stellaris Forth pre-loaded, see [SourceForge][F].
This 16 KB firmware provides a nice runtime environment, everything else here
has been implemented on top in pure Forth.

There are a few small "wrapper" files in this area, which include several more
substantial source files from `../flib/`:

* "h" will install a simple **H**ardware abstraction layer into flash memory
* "l" installs some **L**ibrary packages on top of "h", also in flash memory
* "d" is **D**evelopment and testing code which expects "h" and "l" in flash
* "a" is reserved for "sealed" ready-to-run **A**pplications (not used here)

(with Picocom, "^A ^S d" is very conveniently placed on the home row of keys)

Loading "h" will erase flash (everything but Mecrisp) before re-installing
itself. Similarly, loading "l" leaves Mecrisp and the "h" code intact, but
erases everything else before re-installing "l" (it then ends by loading "d").
Loading "d" is a RAM-only affair, allowing fast and frequent edit-run cycles.

The plan is to put everything deemed _essential_ in "h", while creating several
different versions of "d", depending on the application. In fact, the idea is
to have one folder with at least their own "l" and "d" files for each project.
All generic (or "official", if you prefer) code will be placed in "`../flib/`".
A completely sealed & ready-to-run application should be saved as an "a" file.

In day-to-day use, "h" and "l" get loaded once and then remain on the chip,
ready for use after power-up. This creates a fairly elaborate context for
development, including bit-banged SPI and I2C drivers, as well as the OLED
and RF69 drivers. There is a complete graphics library, able to draw lines,
circles, and text, plus a logo bitmap and an ASCII 8x8 text font.

Flash memory use is well under 32 KB for all of the above plus Mecrisp Forth.

New code and definitions can be typed in interactively, or added to "d" and
reloaded. Once the code is stable enough, it can then be moved to a new source
file and included in "l" to make it more permanent.

## Examples

Here are a few examples, using definitions included in "h" and "l".

Show current GPIO settings:

    io.all 
    PIN 0  PORT A  CRL 14114414  CRH 000004B0  IDR 0459  ODR A010
    PIN 0  PORT B  CRL 44484444  CRH 44444444  IDR 00DA  ODR 0010
    PIN 0  PORT C  CRL 44444444  CRH 44444444  IDR 0000  ODR 0000
    PIN 0  PORT D  CRL 44444444  CRH 44444444  IDR 0000  ODR 0000
    PIN 0  PORT E  CRL 44444444  CRH 44444444  IDR 0000  ODR 0000 ok.

Show GPIO info related to a specific pin:

    led io. 
    PIN 1  PORT A  CRL 14114414  CRH 000004B0  IDR 0459  ODR A010 ok.

Scan the I2C bus for attached devices:

    i2c-init i2c. 
    00: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
    10: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
    20: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
    30: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
    40: -- -- -- -- -- -- -- -- 48 -- -- -- -- -- -- --
    50: -- -- -- 53 -- -- -- -- -- -- -- -- -- -- -- --
    60: -- -- -- -- -- -- -- -- 68 -- -- -- -- -- -- --
    70: -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- ok.

Receive some RF69 test packets (stopped by keypress):

    rfdemo 
    OK 128 24 66 1 2 
    OK 128 24 67 1 2 3 
    OK 128 24 68 1 2 3 4 
    OK 128 24 69 1 2 3 4 5 
    ok.

Receive some RF69 test packets, in HEX format (stopped by keypress):

    rfdemox 
    OKX 801846010203040506
    OKX 80184701020304050607
    OKX 8018480102030405060708
    OKX 801849010203040506070809
    ok.

Dump the RF69's internal register settings:

    rf. 
         0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F 
    00: -- 10 00 02 8A 02 E1 D9 26 40 41 60 02 92 F5 20
    10: 24 9F 09 1A 40 B0 7B 9B 18 4A 42 40 80 06 5C 00
    20: 00 FF F3 00 83 00 07 D9 46 A0 00 00 00 05 88 2D
    30: 2A 00 00 00 00 00 00 D0 42 00 00 00 8F 12 00 00
    40: 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00
    50: 14 C5 88 08 00 00 01 00 1B 09 55 80 70 33 CA 08 ok.

Dump memory contents:

    $4800 128 dump 
    00004800   40 00 03 50 41 35 47 F8   04 6D 05 26 70 47 00 00   @..PA5G. .m.&pG..
    00004810   24 48 00 00 40 00 03 50   41 36 47 F8 04 6D 06 26   $H..@..P A6G..m.&
    00004820   70 47 00 00 38 48 00 00   40 00 03 50 41 37 47 F8   pG..8H.. @..PA7G.
    00004830   04 6D 07 26 70 47 00 00   4C 48 00 00 40 00 03 50   .m.&pG.. LH..@..P
    00004840   41 38 47 F8 04 6D 08 26   70 47 00 00 60 48 00 00   A8G..m.& pG..`H..
    00004850   40 00 03 50 41 39 47 F8   04 6D 09 26 70 47 00 00   @..PA9G. .m.&pG..
    00004860   74 48 00 00 40 00 04 50   41 31 30 00 47 F8 04 6D   tH..@..P A10.G..m
    00004870   0A 26 70 47 88 48 00 00   40 00 04 50 41 31 31 00   .&pG.H.. @..PA11.
    ok.

## Firmware upload

This is a transcript of loading "h" and then "l" into the system with the
[PicoCom][P] terminal utility and [msend][M]:

    Mecrisp-Stellaris 2.2.1 for STM32F103 by Matthias Koch

    *** file: h
    msend h 
    cr eraseflash 
    Finished. Reset !Mecrisp-Stellaris 2.2.1 for STM32F103 by Matthias Koch
        >>> include ../flib/mecrisp/hexdump.fs
        <<<<<<<<<<< ../flib/mecrisp/hexdump.fs (73 lines)
        >>> include ../flib/stm32f1/io.fs
        <<<<<<<<<<< ../flib/stm32f1/io.fs (59 lines)
        >>> include ../flib/stm32f1/hal.fs
        <<<<<<<<<<< ../flib/stm32f1/hal.fs (109 lines)
        >>> include ../flib/stm32f1/adc.fs
        <<<<<<<<<<< ../flib/stm32f1/adc.fs (26 lines)
        <<<<<<<<<<< h (34 lines)

    *** exit status: 0

    *** file: l
    msend l 
    ( cornerstone ) <<<hal-rnw>>> 
    Finished. Reset !Mecrisp-Stellaris 2.2.1 for STM32F103 by Matthias Koch
        >>> include ../flib/any/spi-bb.fs
        <<<<<<<<<<< ../flib/any/spi-bb.fs (26 lines)
        >>> include ../flib/spi/rf69.fs
        <<<<<<<<<<< ../flib/spi/rf69.fs (164 lines)
        >>> include ../flib/any/i2c-bb.fs
        <<<<<<<<<<< ../flib/any/i2c-bb.fs (49 lines)
        >>> include ../flib/i2c/ssd1306.fs
        <<<<<<<<<<< ../flib/i2c/ssd1306.fs (93 lines)
        >>> include ../flib/mecrisp/graphics.fs
        <<<<<<<<<<< ../flib/mecrisp/graphics.fs (247 lines)
        >>> include ../flib/mecrisp/multi.fs
        <<<<<<<<<<< ../flib/mecrisp/multi.fs (206 lines)
    ( code-size ) here swap - . 10016  ok.
    ( flash-end ) here hex. 00007C00  ok.
        >>> include d
    reset UMecrisp-Stellaris 2.2.1 for STM32F103 by Matthias Koch
        <<<<<<<<<<< d (35 lines)
        <<<<<<<<<<< l (28 lines)

    *** exit status: 0

That's it for now. Enjoy!

  [R]: http://jeelabs.org/book/1545f/
  [H]: http://www.hotmcu.com/stm32f103tb-arm-cortex-m3-development-board-p-222.html
  [F]: http://mecrisp.sourceforge.net/
  [M]: https://github.com/jeelabs/embello/tree/master/tools/msend
  [P]: https://github.com/npat-efault/picocom
