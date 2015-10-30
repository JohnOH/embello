// Report incoming RF69 radio packets on the HY-TinySTM103 serial port.

#include <SPI.h>
#include "spi.h"
#include "rf69.h"

RF69<SpiDev> rf;

void setup () {
    Serial.begin(115200);
    Serial.println("[radio]");

    rf.init(1, 42, 8686);
}

void loop () {
    uint8_t buffer [70];
    int n = rf.receive(buffer, sizeof buffer);
    if (n >= 0) {
        Serial.print("got #");
        Serial.print(n);
        Serial.print(':');
        for (int i = 0; i < n; ++i) {
            Serial.print(' ');
            Serial.print(buffer[i]);
        }
        Serial.println();
    }
}
