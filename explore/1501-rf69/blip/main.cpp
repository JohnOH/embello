// Periodically send out a test packet with an incrementing counter.
// See http://jeelabs.org/2014/12/31/lpc810-meets-rfm69/

#define chThdYield() // FIXME still used in radio.h

#include "spi.h"
#include "radio.h"

RF69<SpiDevice> rf;

void sleepSetup () {
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
    LPC_WKT->CTRL = (1<<0);             // WKT_CTRL_CLKSEL

    NVIC_EnableIRQ(WKT_IRQn);

    LPC_SYSCON->STARTERP1 = (1<<15);    // wake up from alarm/wake timer
    LPC_PMU->DPDCTRL = (1<<2);          // LPOSCEN
    LPC_PMU->PCON = (2<<0);             // power down, but not deep
}

extern "C" void WKT_IRQHandler () {
    LPC_WKT->CTRL |= (1<<1) | (1<<2);   // clear alarm
}

void sleep (int millis) {
    LPC_WKT->COUNT = 10 * millis;       // start counting at 10 KHz
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode

    __WFI(); // wait for interrupt, powers down until the timer fires

    SCB->SCR &= ~(1<<2);                // disable SLEEPDEEP mode
}

int main () {
    LPC_SWM->PINENABLE0 |= (3<<2) | (1<<6); // disable SWCLK/SWDIO and RESET

    // NSS=2, SCK=3, MISO=5, MOSI=1
    LPC_SWM->PINASSIGN3 = 0x03FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF020501;   // -    nss  miso mosi

    sleepSetup();

    rf.init(1, 42, 8683);
    rf.encrypt("mysecret");
    rf.txPower(0); // minimal

    int cnt = 0;
    while (true) {
        rf.send(0, &++cnt, sizeof cnt); // send out one packet
        rf.sleep();
        sleep(1000);                    // power down for 1 second
    }
}
