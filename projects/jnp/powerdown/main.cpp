// Put attached RFM69 radio and the processor in total power-down mode.

#include "embello.h"

#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

int main () {
  LPC_SWM->PINASSIGN[3] = 0x11FFFFFF;
  LPC_SWM->PINASSIGN[4] = 0xFF170908;

  rf.init(61, 42, 8686);
  rf.sleep();

  SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
  LPC_PMU->DPDCTRL |= (1<<2)|(1<<3);  // LPOSCEN and LPOSCDPDEN
  LPC_PMU->PCON = 3;                  // enter deep power-down mode

  __WFI();                            // wait for interrupt, powers down
}
