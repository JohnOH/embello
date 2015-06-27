// Blink the Pico Emu's red LED and send a message to the serial port.

#include "embello.h"

void emuInit (const char* name) {
  tick.init(1000);
  serial.init(115200);
  printf("\n[pem/%s]\n", name);

  LPC_GPIO_PORT->DIR[0] |= (1<<13); // set 13p1 as output
}

void emuLed (bool on) {
  LPC_GPIO_PORT->B[0][13] = on;
}

int main () {
    emuInit("1-blink");

    emuLed(true);

    while (true) {
        printf("ping\n");
        LPC_GPIO_PORT->NOT[0] = 1<<13; // toggle the red LED
        tick.delay(500);
    }
}
