// Command line interface using the serial port.

#include "sys.h"
#include "uart_irq.h"

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[cmder]\n");

  while (true) {
    int ch = uart0RecvChar();
    if (ch >= 0)
      printf("%d\n", ch);
  }
}
