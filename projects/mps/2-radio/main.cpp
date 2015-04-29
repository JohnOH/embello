// Micro Power Snitch code, including periodic radio packet transmissions.
// See http://jeelabs.org/2015/03/11/micro-power-snitch-part-4/

#include "sys.h"

#define chThdYield() // FIXME still used in rf69.h
#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

static void sleep (int ticks) {
  LPC_WKT->COUNT = ticks;             // sleep time in 0.1 ms steps
  SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
  __WFI();
  SCB->SCR &= ~(1<<2);                // disable SLEEPDEEP mode
}

int main () {
  LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
  LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL

  NVIC_EnableIRQ(WKT_IRQn);

  LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
  LPC_PMU->DPDCTRL = (1<<2) | (1<<1); // LPOSCEN, no wakepad
  LPC_PMU->PCON = 2;                  // use normal power-down mode

  sleep(10000); // sleep 1 s to let power supply rise further

  // disable all special pin functions
  LPC_SWM->PINENABLE0 = ~0;

  // turn power to the radio on
  LPC_GPIO_PORT->B[0][1] = 0;         // low turns on radio power
  LPC_GPIO_PORT->DIR[0] |= 1<<1;      // PIO0_1 is an output

  sleep(100); // sleep 10 ms to let the radio start up

  // SPI0 pin configuration
  // lpc810: sck=3p3, ssel=4p2, miso=2p4, mosi=5p1
  LPC_SWM->PINASSIGN[3] = 0x03FFFFFF;   // sck  -    -    -
  LPC_SWM->PINASSIGN[4] = 0xFF040205;   // -    nss  miso mosi

  // initialise the radio and put it into idle mode asap
  rf.init(61, 42, 8683);              // node 61, group 42, 868.3 MHz
  rf.sleep();

  // configure the radio a bit more
  rf.encrypt("mysecret");
  rf.txPower(0); // 0 = min .. 31 = max

  sleep(5000); // sleep 500 ms

  while (true) {
    // send out one packet and go back to sleep
    rf.send(0, "xyz", 3);
    rf.sleep();

    sleep(30000); // sleep 3 sec
  }
}

extern "C" void WKT_IRQHandler () {
  LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
  // LPC_WKT->CTRL |= (1<<1) | (1<<2);   // clear alarm
}
