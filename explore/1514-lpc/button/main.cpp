// Use a pin change interrupt to detect a button press.

#include "embello.h"

volatile bool triggered;

extern "C" void PIN_INT0_IRQHandler () {
  LPC_PININT->IST = (1<<0);       // clear interrupt
  triggered = true;
}

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[button]\n");

  LPC_SWM->PINENABLE0 |= (3<<2);  // disable SWCLK/SWDIO
  // PIO0_2 is already an input with pull-up enabled
  LPC_SYSCTL->PINTSEL[0] = 2;     // pin 2 triggers pinint 0
  LPC_PININT->SIENF = (1<<0);     // enable falling edge
  NVIC_EnableIRQ(PININT0_IRQn);

  while (true) {
    if (triggered) {
      triggered = false;
      printf("%u\n", (unsigned) tick.millis);
    }
  }
}
