# USB Serial Uploader

Usbserup is a boot loader for use with STM32F103 microcontrollers. It listens on
the USB serial port for incoming upload requests and stores the data in flash.
When done, or when no further requests are received, the new code is started up.

The upload protocol is the same as STM's own serial upload format, so the same
tools can be used as with boot-ROM serial (stm32loader.py, stm32flash, etc).

Note: usbserup initialises the USB device but does not set up or use interrupts.

## Status

This is work in progress (meaning: please test, comment, and help improve it!).

Usbserup currently has some hard-coded assumptions:

* pin PA1 is low as long as the boot loader runs (for debugging with an LED)
* pin PA0 is used as USB disconnect (same idea as Maple Mini, but different pin)
* it's loaded at 0x08001C000, i.e. in the top 16 KB of a 128 KB flash device
  (for some reason, installation in the top 8K failed, this is a TODO...)

## High-flash

This boot loader normally resides in _high_ flash memory. This avoids having to
build your uploaded code to run at a different starting address, i.e. you can
upload and run the same code compiled to start at 0x08000000 regardless of
whether you use the serial USB uploader or STM's built-in serial boot ROM.

In other words: your code will run in the same place as always, with `usbserup`
residing at the top of flash memory (consuming the last few KB).

This requires some smoke-and-mirror tricks:

* all requests to erase flash memory are ignored (a full erase would be fatal)
* instead, usbserup erases a page _only_ just before it starts to write to it
* the boot vector jump at 0x08000004 _must_ point to the usbserup boot loader,
  for it to (re-) gain control at all times (after power-up and after a reset)
* when usbserup stores your code in flash, it will modify the first eight bytes:
    * bytes 0..3 (address 0x08000000) will contain your code's start address
    * bytes 4..7 (address 0x08000004) will be a jump to the usbserup loader
* this patching is done on-the-fly and transparantly by usbserup, just before
  saving your code to flash memory
* there is one important requirement: since bytes 0..3 are overwritten, the
  original starting stack pointer address will be lost - your code must
  set up its stack pointer on startup, to make sure it has the expected value

To summarise: usbserup lives in high flash (at 0x08020000 for a 128 KB µC), and
also requires the first 8 bytes of flash memory to hold specific values. Apart
from that, there are no differences. Your code runs at 0x08000000, as usual.

## Installation

Build instructions:

    cd tools/usbserup
    git submodule update --init
    cd libopencm3
    make
    cd ..
    make

One tricky bit is getting this code into the correct area of flash memory, and
then getting it started up. This can be done via the "installboot.ino" sketch,
see `embello/projects/ask/installboot/`. This is a normal sketch which you need
to get running first, using whatever upload mechanism you already have working.

## How it works

A copy of usbserup is included as data inside installboot, i.e. the build for
usbserup ends by generating a "data.h" file, for inclusion in `installboot.ino`.

Installboot will copy usbserup to the proper location in flash and jump to it.
At this point, the first 8 bytes of flash are still wrong, but they'll be fixed
the very first time an actual upload request is processed by usbserup. 

After that, installboot will be gone and usbserup stays loaded in high-flash.
Usbserup ignores requests to overwrite itself to avoid corrupting its own code.

## License

This code started off as the [USB CDC-ACM][1] example code on GitHub.  
License: LGPL3, like [libopencm3][2] + [libopencm3-example][3] it's based on.

[1]: https://github.com/libopencm3/libopencm3-examples/tree/master/examples/stm32/f1/stm32-maple/usb_cdcacm
[2]: https://github.com/libopencm3/libopencm3
[3]: https://github.com/libopencm3/libopencm3-examples
