// ADC implementation using analog comparator with State Configurable timer.

#include "embello.h"
#include "adc_sct.h"

AdcSct<1> adc;  // use ACMP_I1

extern "C" void SCT_IRQHandler () { adc.sctIrqHandler(); }

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[sigmadc]\n");

  // LPC_IOCON->PIO0[0] = 0x80; // disable pull-up on ACMP input pin
  LPC_SWM->PINENABLE0 |= (3<<2);      // disable SWDIO and SWCLK
  LPC_SWM->PINASSIGN[8] = 0xFFFF02FF; // enable ACMP_O on PIO_2
  LPC_SWM->PINASSIGN[5] = 0x02FFFFFF; // enable CTIN_0 on PIO_2

  adc.init();

  while (true) {
    tick.delay(500);
    printf("%d\n", adc.result());
  }
}
