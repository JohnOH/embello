This area is the "Test Echo Node", used for testing JeeNode Zero boards. Waits
for incoming RF69 packets and sends replies back with reception RSSI info, etc.

Written for F103, runs on a Blue Pill with RFM69 connected to PA4..PA7, and an
LED assumed to be on PC13. It's all very easy to change in `board.fs`.

This runs on top of a USB-enabled Mecrisp and also reports its activity via USB.
