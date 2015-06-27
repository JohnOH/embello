// Report 12-bit analog measurements over serial twice per second.

#include "embello.h"

int main () {
  tick.init(1000);
  serial.init(115200);
  analog.init();

  printf("\n[analog]\n");

  while (true) {
    int adc = analog.measure(10); // adc10 is 13p3 on LPC824
    printf("%d\n", adc);
    tick.delay(500);
  }
}
