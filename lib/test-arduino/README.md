To compile and run this demo with the Arduino IDE:

* create a new folder called "rf69demo" in your "Sketchbook" folder
* copy the following files to it:

        rf69demo.ino
        ../arch-arduino/spi.h
        ../driver/rf69.h

Then build and upload as usual using the IDE.

Note: this code defines an "RF69" class which is not related to the "RF69"
compatibility-mode class in JeeLib. The difference is that this version
will configure the RFM69 radio module to use _native_ packet mode, whereas
the JeeLib version is a hack to let the RFM69 send and receive RF12-type data.
