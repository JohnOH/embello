// Stay in deep power-down mode for a minute to allow current measurements.
// See http://jeelabs.org/2014/11/26/getting-started-final-episode/

#include "LPC8xx.h"

int main () {
    // comment out the next two lines to disable the watchdog
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
    LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL

    NVIC_EnableIRQ(WKT_IRQn);

    LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
    LPC_PMU->DPDCTRL |= (1<<2)|(1<<3);  // LPOSCEN and LPOSCDPDEN
    LPC_PMU->PCON = 3;                  // enter deep power-down mode

    for (int count = 0; count < 900000; ++count)
        __ASM("");                      // waste time while drawing current

    LPC_WKT->COUNT = 600000;            // 10 KHz / 600000 -> wakeup in 60 s
    __WFI();                            // wait for interrupt, powers down
}

extern "C" void WKT_IRQHandler () {
    LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
}
