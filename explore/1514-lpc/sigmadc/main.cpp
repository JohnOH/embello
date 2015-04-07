// ADC implementation using analog comparator with State Configurable timer.

#include "sys.h"

#include "adc_sct.h"

AdcSct<2> adc;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[sigmadc]\n");

  // LPC_IOCON->PIO0[1] = 0x80; // disable pull-up on ACMP input pin
  LPC_SWM->PINASSIGN[8] = 0xFFFF07FF; // enable ACMP_O on PIO_7
  LPC_SWM->PINASSIGN[5] = 0x07FFFFFF; // enable CTIN_0 on PIO_7

  adc.init();

  while (true) {
    tick.delay(500);
    printf("%d\n", adc.result());
  }
}

extern "C" void SCT_IRQHandler () {
  adc.sctIrqHandler();
}
