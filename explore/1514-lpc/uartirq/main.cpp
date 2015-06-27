// Simple hello world baseline code, using interrupts with a ring buffer.

#include "embello.h"
#include "uart_irq.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[uartirq]\n");

  // this string exceeds the 15-byte capacity of the output ring buffer
  printf("123456789 123456789 123456789 123456789 123456789\n");

  while (true) {
    tick.delay(500);
    printf("%u\n", (unsigned) tick.millis);
  }
}
