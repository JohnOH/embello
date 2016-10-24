// Periodically fade the LED by modulating total current consumption.
// See http://jeelabs.org/2014/11/13/getting-started-episode-2/

#include "LPC8xx.h"

// do something deemed useful, then return the number of 100 us cycles to sleep
// after that, main will be restarted and call this again, hence the name "loop"
static int loop () {
    const int LIMIT = 100;              // gives approx 1s cycle time
    int n = ++LPC_PMU->GPREG0;          // this register persists across resets
    if (n > LIMIT)                      // make it increment from 1 to LIMIT
        n = LPC_PMU->GPREG0 = 1;        // back to 1 when limit is exceeded

    // idle loop to keep the LED turned on for approx (LIMIT-n) * 100 us
    for (int count = 0; count < 180 * (LIMIT - n); ++count)
        __ASM("");                      // waste time while drawing current

    return n;   // now go to sleep and keep the LED turned off for n * 100 us
}

int main () {
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
    LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL

    NVIC_EnableIRQ(WKT_IRQn);

    LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
    LPC_PMU->DPDCTRL |= (1<<2)|(1<<3);  // LPOSCEN and LPOSCDPDEN
    LPC_PMU->PCON = 3;                  // enter deep power-down mode

    LPC_WKT->COUNT = loop();            // do some work, then set sleep counter
    __WFI();                            // wait for interrupt, powers down

    // waking up from deep power-down leads to a full reset, no need to loop
}

extern "C" void WKT_IRQHandler () {
    LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
}
