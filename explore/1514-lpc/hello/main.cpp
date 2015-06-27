// Simple hello world baseline code, for smoke testing and size reference.

#include "embello.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[hello]\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", (unsigned) tick.millis);
  }
}
