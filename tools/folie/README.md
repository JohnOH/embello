# Folie v1

The **Fo**rth **Li**ve **E**xplorer is a serial terminal interface for [Mecrisp Forth][MF]:

* connects as terminal console to an attached microcontroller board over serial
* can also connect to a telnet socket or to ssh, launched as a subprocess
* line entry: each line is sent out after a return, and output displayed back
* command history: uses [readline][RL] to edit and re-send previous commands
* include files: each line of the form "`include <filename>`" is processed  
  as a request to send the contents of that file as if it had been typed in
* nested includes: `include` lines found inside are processed recursively
* throttling: each line waits for an "`ok.`" prompt before sending the next one
* firmware uploads: allows erasing an STM32F1 chip and uploading new firmware
* the default baud rate is 115200, see `folie -help` for a list of options

> Note: there's a new [Folie v2][F2] in development (work-in-progress for now).

### Installation

The latest binaries can be found in the release area on [GitHub][GH].  
Alternately, get the [source code][SC] and run `make app` from `tools/folie/`.

### Windows

Download, uncompress, and rename to "`folie.exe`".  
Launch as "`folie -p COM3`", assuming the board is connected to COM3.  

_Warning: the Prolific PL2303 USB serial adapters may not work... (TODO)_

### Mac OSX

Download, uncompress, rename to "`folie`", and do a "`chmod +x folie`".  
Launch as "`./folie -p /dev/cu.SLAB_USBtoUART`" or whatever the device name is.

### Linux

Download, uncompress, rename to "`folie`", and do a "`chmod +x folie`".  
Launch as "`./folie -p /dev/ttyAMA0`" or whatever the device name is.

### Keyboard shortcuts

* `return` - send command to remote µC
* `ctrl-a` - go to start of line
* `ctrl-e` - go to end of line
* `up-arrow`, `ctrl-p` - previous command in history
* `down-arrow`, `ctrl-n` - next command in history
* `ctrl-r` / `ctrl-s` - backward / forward history search
* `ctrl-c`, `ctrl-d` - exit Folie

See the [readline]() page for a complete list of all the supported shortcuts.

### Connecting to a telnet socket

This mode can be used in combination with an [ESP-Link][EL] WiFi to serial  
bridge, by replacing the serial device name with "`<dns-or-ip>:23`".

### Connecting via SSH

To run Folie as front-end for a Linux-based version of Mecrisp, use the  
following incantation: "`folie -x ssh <hostname> <path/to/mecrisp>`".

On Linux, this same `-x` option can also be used to launch Mecrisp locally,  
using: "`folie -x <path/to/mecrisp>`".

### Firmware uploads

When started with the `-u` option, Folie will try to upload a firmware image  
into an STM32F103 chip, using its built-in boot ROM protocol. For example:

    folie -p <port> -u mecrisp-stellaris-stm32f103.bin

To use this mode, the chip needs to be placed in "boot mode", i.e. reset with  
the `BOOT0` pin tied high (this differs for each board, it's often a jumper).  
Note that the chip's flash contents will be _completely_ erased and replaced!

When done: restore the jumper, press reset, and re-launch without `-u` flag.

### Known problems

The last output line(s) can get overwritten when using history search.  
Under Windows, Prolific PL2303 chips are causing deadlocks - no idea why.

### License

MIT, see also [chzyer/readline][LR] and [tarm/serial][LS] included in this app.

  [MF]: http://mecrisp.sourceforge.net
  [RL]: http://gopkg.in/readline.v1
  [GH]: https://github.com/jeelabs/embello/releases
  [SC]: https://github.com/jeelabs/embello
  [LR]: https://github.com/chzyer/readline/blob/v1.2/LICENSE
  [LS]: https://github.com/tarm/serial/blob/master/LICENSE
  [EL]: https://github.com/jeelabs/esp-link
  [F2]: https://github.com/jeelabs/folie
