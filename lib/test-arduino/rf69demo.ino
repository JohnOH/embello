// Report received data on the serial port.
#include <Arduino.h>
#include <stdint.h>

#include "spi.h"
#include "rf69.h"

RF69<SpiDev10> rf;

uint8_t rxBuf[64];
uint8_t txBuf[62];
uint16_t cnt = 0;

void setup () {
  Serial.begin(57600);
  Serial.println("\n[rf69demo]");

  rf.init(28, 42, 8686);
  //rf.encrypt("mysecret");
  rf.txPower(15); // 0 = min .. 31 = max

  for (int i = 0; i < (int) sizeof txBuf; ++i)
    txBuf[i] = i;
}

void loop () {
  if (++cnt == 0) {
    int txLen = ++txBuf[0] % (sizeof txBuf + 1);
    Serial.print(" > #");
    Serial.print(txBuf[0]);
    Serial.print(", ");
    Serial.print(txLen);
    Serial.println("b");
    rf.send(0, txBuf, txLen);
  }

  int len = rf.receive(rxBuf, sizeof rxBuf);
  if (len >= 0) {
    Serial.print("OK ");
    for (int i = 0; i < len; ++i) {
      Serial.print(rxBuf[i] >> 4, HEX);
      Serial.print(rxBuf[i] & 0xF, HEX);
    }
    Serial.print(" (");
    Serial.print(rf.rssi);
    Serial.print(rf.afc < 0 ? "" : "+");
    Serial.print(rf.afc);
    Serial.print(":");
    Serial.print(rf.lna);
    Serial.println(")");
  }
}
