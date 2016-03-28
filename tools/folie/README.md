# Folie

The **Fo**rth **Li**ne **E**valuator is a serial terminal interface for Mecrisp Forth:

* connects to an embedded µC over serial
* line entry: each line is sent out over serial, and output displayed back
* command history: uses [readline][RL] to edit and re-send previous commands
* include files: each line of the form "`include <filename>`" is processed  
  as a request to send the contents of that file as if it had been typed in
* nested includes: `include` lines found inside are also processed recursively
* throttling: each line waits for the "ok." prompt before sending the next one
* the default baud rate is 115200, see `folie -help` for a list of options

### Installation

The latest binaries can be found in the release area on [GitHub][GH].  
Alternately, get the [source code][SC] and run `make` from `tools/folie/`.

### Windows

Launch as "`folie -p COM3`", assuming the board is connected to COM3.

### Mac OSX

Launch as "`folie -p /dev/cu.SLAB_USBtoUART`" or whatever the device name is.

### Linux

Launch as "`folie -p /dev/ttyAMA0`" or whatever the device name is.

### Keyboard shortcuts

* `up-arrow`, `ctrl-p` - previous command in history
* `down-arrow`, `ctrl-n` - next command in history
* `ctrl-a` - go to start of line
* `ctrl-e` - go to end of line
* `return` - send command
* `ctrl-c`, `ctrl-d` - exit Folie

See the [readline]() page for a complete list of all the supported shortcuts.

### Known problem

The last lines can get messed up when using the up/down arrow keys.

### License

MIT, see also [chzyer/readline][LR] and [tarm/serial][LS] included in this app.

  [RL]: http://gopkg.in/readline.v1
  [GH]: https://github.com/jeelabs/embello/releases
  [SC]: https://github.com/jeelabs/embello
  [LR]: https://github.com/chzyer/readline/blob/v1.2/LICENSE
  [LS]: https://github.com/tarm/serial/blob/master/LICENSE
