# JeeBoot

An implementation of over-the-air software updates for JeeNodes:

* the bulk of the code lives in `.h` files in `include/`
* tests are located in the `.cpp` files in `tests/`
* there's a dummy source file in `src/` to keep the `ar` command happy

This structure is needed to support [CppUTest][1] without changes.

## Overview

Over-the-air uploading requires two pieces of software to work together:

1. A boot **loader** which communicates over RF and re-flashes the µC as needed.
2. A boot **server** which listens for boot requests and sends out the code.

The loader for the RFM69 on LPC824 can be found in `tools/jeeboot/loader69/`.
Just connect the LPC824 with FTDI and type make to build and upload this boot
loader. The upload precedure is a bit special, since the loader needs to end up
in high flash memory to work properly.

The boot server requires a [RasPi RF][3] interface, which connects an RFM69
wireless module to a Raspberry Pi or Odroid C1. The code which handles boot
requests and passes everything through to MQTT is called "rf69bridge" and
can be found in `projects/rpr/rf69bridge/`. It runs on Linux and needs the
WiringPi and Mosquitto libraries to build properly.

Once properly setup, typing `make` in both directories should handle everything.
Here is the boot loader - built in debug mode, then uploaded and running:

    $ make
    mkdir -p build/obj
    [...]
    uploader -w -o 28672 /dev/tty.usbserial-A600K1PM build/firmware.bin
    found: 8242 - LPC824: 32 KB flash, 8 KB RAM, TSSOP20
    hwuid: 3F700407679C61AEDE84A053870200F5
    flash: 0D80 done, 3436 bytes
    uploader -s /dev/tty.usbserial-A600K1PM build/firmware.bin
    found: 8242 - LPC824: 32 KB flash, 8 KB RAM, TSSOP20
    hwuid: 3F700407679C61AEDE84A053870200F5
    flash: 0D80 done, 3436 bytes
    entering terminal mode, press <ESC> to quit:

    [loader]
    rf inited 10000008
    hwid 0704703f ae619c67 53a084de f5000287
    > identify
    request # 18
      got # 10
      swid 65504 size 2572 crc 30e1
      myCrc 4b2c
    > fetchAll 65504
    request # 4
    [...]

And this is the server side, built, launched, and showing a full upload cycle:

    $ make
    g++ [...]
    sudo ./rf69bridge files/

    [rf69bridge] raw/rf69/8686-42 @ 127.0.0.1 using: files/*
    sending 18 -> 8 bytes
    sending 4 -> 54 bytes
    sending 4 -> 2 bytes
    sending 18 -> 8 bytes

(lines "`sending 4 -> 62 bytes`" are not printed to reduce the amount of output)

To see the actual incoming data via MQTT, you can use a Mosquitto utility:

    $ mosquitto_sub -v -t '#'
    raw/rf69/8686-42/0 "14008b0380c063103f700407679c61aede84a053870200f5"
    raw/rf69/8686-42/0 "10008e0380c0e0ff0000"
    [...]
    raw/rf69/8686-42/0 "0a008b0380c0e0ff2b00"
    raw/rf69/8686-42/0 "12008b0380c063103f700407679c61aede84a053870200f5"
    raw/rf69/8686-42/24 "14008c03801801"
    raw/rf69/8686-42/24 "12008b0380180201"
    raw/rf69/8686-42/24 "1a008b038018030102"

This shows some boot packets (origin "0") and then three sample packets from
the `rf_test` demo, once its code has been succesfully uploaded and started.

## Testing

The main JeeBoot code was written using [Test-Driven Development][2] techniques.

To run the  tests, set the `CPPUTEST_HOME` environment variable to point to an
installed copy of CppUTest (version 3.7.1 is known to work), then type:

    cd tools/jeeboot/tests
    make

Sample output for a succesful test run:

    Running jeeboot_tests
    .....................
    OK (21 tests, 21 ran, 52 checks, 0 ignored, 0 filtered out, 1 ms)

[1]: http://cpputest.github.io
[2]: https://en.wikipedia.org/wiki/Test-driven_development
[3]: http://jeelabs.org/2015/05/20/rfm69-on-raspberry-pi/
