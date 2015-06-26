# JeeBoot

An implementation of over-the-air software updates for JeeNodes:

* the bulk of the code lives in `.h` files in `include/`
* tests are located in the `.cpp` files in `tests/`
* there's a dummy source file in `src/` to keep the `ar` command happy

This structure is needed to support [CppUTest][1] without changes.

## Overview

Over-the-air uploading requires two pieces of software to work together:

1. A boot **loader** which communicates over RF and reflashes the µC as needed.
2. A boot **server** which listens for boot requests and sends out the code.

The boot loader for the RFM69 on LPC824 can be found in `jeeboot/loader69/`.
Just connect the LPC824 with FTDI and type make to build and upload this boot
loader. The upload precedure is a bit special, since the loader needs to end up
in high flash memory to work properly.

The boot server requires a [RasPi RF][3] interface, which connects an RFM69
wireless module to a Raspberry Pi or Odroid C1. The code which handles boot
requests and passes everything through to MQTT is called _rf69bridge_ and
can be found in `embello/projects/rpr/rf69bridge/`. It runs on Linux and needs
the WiringPi and Mosquitto libraries to build properly.

Once properly setup, typing `make` in both directories should handle everything.

## Testing

The main JeeBoot code was written using [Test-Driven Development][2] techniques.

To run the  tests, set the `CPPUTEST_HOME` environment variable to point to an
installed copy of CppUTest (version 3.7.1 is known to work), then type:

    cd tests
    make

Sample output for a succesful test run:

    Running jeeboot_tests
    ..........
    OK (10 tests, 10 ran, 24 checks, 0 ignored, 0 filtered out, 0 ms)

[1]: http://cpputest.github.io
[2]: https://en.wikipedia.org/wiki/Test-driven_development
[3]: http://jeelabs.org/2015/05/20/rfm69-on-raspberry-pi/
