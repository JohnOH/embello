# lpc8xx

The **lpc8xx** utility is a simple uploader and terminal app for LPC8xx chips.
The LPC8xx is assumed to be connected via a [modified FTDI interface][BUB].

## Installation

The following applies to Mac OSX and Linux:

* make sure Go has been [installed][GO] and GOPATH has been set up
* enter this command: `go get github.com/jeelabs/embello/tools/lpc8xx`
* the resulting binary will be available as `$GOPATH/bin/lpc8xx`

## Usage

Report the chip type and its serial id using interface "/dev/ttyUSB0":

    lpc8xx /dev/ttyUSB0

Upload "firmware.bin" to the LPC8xx board:

    lpc8xx /dev/ttyUSB0 firmware.bin

Upload, waiting for the board to be inserted:

    lpc8xx -w /dev/ttyUSB0 firmware.bin

Upload, connect as terminal after the upload (at 115200 baud):

    lpc8xx -t /dev/ttyUSB0 firmware.bin

Upload, connect as terminal, quit when the connection is idle for 30 seconds:

    lpc8xx -t -i 30 /dev/ttyUSB0 firmware.bin

[BUB]: http://jeelabs.org/book/1446d/
[GO]: http://golang.org/doc/install
