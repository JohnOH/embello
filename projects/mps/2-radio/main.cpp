// Micro Power Snitch code, including periodic radio packet transmissions.
// See http://jeelabs.org/2015/03/11/micro-power-snitch-part-4/

#include "LPC8xx.h"

#define chThdYield() // FIXME still used in rf69.h
#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

int main () {
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
    LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL

    NVIC_EnableIRQ(WKT_IRQn);

    LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
    SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
    LPC_PMU->DPDCTRL = 0xE;             // no wakeup, LPOSCEN and LPOSCDPDEN
    LPC_PMU->PCON = 3;                  // enter deep power-down mode

    // GPREG0 is saved across deep power-downs, starts as 0 on power-up reset
    switch (LPC_PMU->GPREG0++) {

        case 0: // do nothing, just let the energy levels build up
            LPC_WKT->COUNT = 30000;         // sleep 3 sec
            break;

        case 1: // turn on power to the radio
            LPC_GPIO_PORT->DIR0 |= 1<<1;    // PIO0_1 is an output
            LPC_GPIO_PORT->B0[1] = 0;       // low turns on radio power

            LPC_WKT->COUNT = 100;           // sleep 10 ms
            break;

        case 2: // initialise the radio and put it to sleep
            // disable SWCLK/SWDIO and RESET                                     
            LPC_SWM->PINENABLE0 |= (3<<2) | (1<<6);                              
            // lpc810: sck=3p3, ssel=4p2, miso=2p4, mosi=5p1
            LPC_SWM->PINASSIGN3 = 0x03FFFFFF;   // sck  -    -    -              
            LPC_SWM->PINASSIGN4 = 0xFF040205;   // -    nss  miso mosi           

            rf.init(61, 42, 8683);          // node 61, group 42, 868.3 MHz
            rf.sleep();

            LPC_WKT->COUNT = 10000;         // sleep 1 sec
            break;

        default: // send out one packet and go back to sleep
            rf.send(0, "xyz", 3);
            rf.sleep();

            LPC_WKT->COUNT = 100000;        // sleep 10 sec
            break;
    }

    __WFI();                            // wait for interrupt, powers down
    // waking up from deep power-down leads to a full reset, no need to loop
}

extern "C" void WKT_IRQHandler () {
    LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
}
