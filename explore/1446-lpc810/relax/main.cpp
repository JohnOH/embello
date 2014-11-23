// Immediate total power down, for current measurements.
// See http://jeelabs.org/2014/11/26/getting-started-final-episode/

#include "LPC8xx.h"

int main () {
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
    LPC_PMU->PCON = 3;                  // enter deep power-down mode
    __WFI();                            // wait for interrupt, powers down
}
