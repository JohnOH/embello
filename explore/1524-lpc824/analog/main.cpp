// Repeatedly perform a 12-bit analog measurement.

#include "sys.h"

int main () {
  tick.init(1000);
  serial.init(115200);
  // LPC_IOCON->PIO0[IOCON_PIO13] = 0; // disable pullups, etc
  // LPC_IOCON->PIO0[IOCON_PIO10] = (1<<8); // disable I2C pin
  analog.init();

  LPC_GPIO_PORT->DIR[0] |= (1<<14); // set 14p20 as output

  printf("\n[analog]\n");

  while (true) {
    int adc = analog.measure(10); // adc10 is 13p3 on LPC824
    // int adc = analog.measure(2); // adc2 is 14p20 on LPC824

    printf("%u\n", adc);
    tick.delay(100);

    LPC_GPIO_PORT->NOT[0] = 1<<14; // toggle pin to expose ADC rate
  }
}
