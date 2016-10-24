// Blink briefly, then go into deep power down for a while, rinse and repeat.
// See http://jeelabs.org/2015/02/18/micro-power-snitch/

#include "LPC8xx.h"

// do something deemed useful, this is the actual "payload" of this application
// after that, main will be restarted and call this again, hence the name "loop"
static void loop () {
    if (LPC_PMU->GPREG0) {
        // idle loop to keep the LED turned on
        for (int count = 0; count < 1000; ++count)
            __ASM(""); // waste time while drawing current
    }

    LPC_PMU->GPREG0 ^= 1; // toggle a bit which will be saved across resets
}

int main () {
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
    LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL

    NVIC_EnableIRQ(WKT_IRQn);

    LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
    LPC_PMU->DPDCTRL = 0xE;             // no wakeup, LPOSCEN and LPOSCDPDEN
    LPC_PMU->PCON = 3;                  // enter deep power-down mode
    LPC_WKT->COUNT = 30000;             // set sleep counter to 3 seconds

    loop();                             // do some work

    __WFI();                            // wait for interrupt, powers down
    // waking up from deep power-down leads to a full reset, no need to loop
}

extern "C" void WKT_IRQHandler () {
    LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
}
