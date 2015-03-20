// Repeatedly perform a 12-bit analog measurement.

#include "sys.h"

int main () {
  tick.init(1000);
  serial.init(115200);
  // LPC_IOCON->PIO0[IOCON_PIO13] = 0; // disable pullups, etc
  // LPC_IOCON->PIO0[IOCON_PIO10] = (1<<8); // disable I2C pin
  analog.init();

  printf("\n[hello]\n");

  while (true) {
    tick.delay(100);
    int adc = analog.measure(10); // adc10 is 13p3 on LPC824
    printf("%u\n", adc);
  }
}
