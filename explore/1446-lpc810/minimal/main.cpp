// Blink the LED by modulating total current consumption.
// See http://jeelabs.org/2014/11/13/getting-started-episode-2/

#include "LPC8xx.h"

int main () {
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
    LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL

    NVIC_EnableIRQ(WKT_IRQn);

    LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
    LPC_PMU->DPDCTRL |= (1<<2)|(1<<3);  // LPOSCEN and LPOSCDPDEN
    LPC_PMU->PCON = 3;                  // enter deep power-down mode

    for (int count = 0; count < 900000; ++count)
        __ASM("");                      // waste time while drawing current

    LPC_WKT->COUNT = 5000;              // 10 KHz / 5000 -> wakeup in 500 ms
    __WFI();                            // wait for interrupt, powers down

    // waking up from deep power-down leads to a full reset, no need to loop
    __builtin_unreachable();            // yak shaving: 4 bytes less ;)
}

extern "C" void WKT_IRQHandler () {
    LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
}
