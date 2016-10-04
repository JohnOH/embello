See <http://jeelabs.org/2016/10/stm32f103-emulating-a-pdp-8/>.

The `focal.h` file was generated from `focal.bin` with this command:

    xxd -i <focal.bin >focal.h

With `focal.bin` being a copy of DEC's official release on FOCAL-69 paper tape.

To run this code:

1. connect an STM32F103-based board via JTAG and with a serial link  
   (for example a HY-Tiny with a Black Magic Probe)
2. adjust the `BMP_PORT` setting in `Makefile.include` accordingly
3. set up a 115200 baud terminal for the serial link
4. make sure there's a `focal.h` file in this directory (see above)
5. run `make flash` to compile and upload the PDP-8 emulator
6. watch the messages on the serial link, reporting Focal's startup
7. enjoy
