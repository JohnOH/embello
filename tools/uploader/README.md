# Uploader

The **uploader** utility is a simple uploader and terminal app for LPC8xx chips.  
The LPC8xx is assumed to be connected via a [modified FTDI interface][BUB].

## Installation

The following applies to Mac OSX and Linux:

* make sure Go has been [installed][GO] and GOPATH has been set up
* enter this command: `go get -u github.com/jeelabs/embello/tools/uploader`
* the resulting binary will be available as `$GOPATH/bin/uploader`

## Usage

Report the chip type and its serial id using interface "/dev/ttyUSB0":

    uploader /dev/ttyUSB0

Upload "firmware.bin" to the LPC8xx board:

    uploader /dev/ttyUSB0 firmware.bin

Upload, waiting for the board to be inserted:

    uploader -w /dev/ttyUSB0 firmware.bin

Upload, connect as terminal after the upload (at 115200 baud):

    uploader -s /dev/ttyUSB0 firmware.bin

Upload, connect as terminal, quit when the connection is idle for 5 seconds:

    uploader -s -i 5 /dev/ttyUSB0 firmware.bin

Upload "firmware.bin" to a remote telnet system, such as [ser2net][S2N]:

    uploader -t 192.168.1.123:4000 firmware.bin

Whereby the ser2net config is assumed to have a entry of the following form:

    4000:telnet:600:/dev/ttyUSB0:115200 remctl

[BUB]: http://jeelabs.org/book/1446d/
[GO]: http://golang.org/doc/install
[S2N]: http://sourceforge.net/projects/ser2net/
