// Micro Power Snitch code, including periodic radio packet transmissions.
// See http://jeelabs.org/mps for information about this project.

#include "embello.h"

#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

static void sleep (int ticks) {
  LPC_WKT->COUNT = ticks;             // sleep time in 0.1 ms steps
  __WFI();
}

int main () {
  // enable wake-ups via low-power watchdog interrupts
  LPC_SYSCON->SYSAHBCLKCTRL |= 1<<9;  // SYSCTL_CLOCK_WKT
  LPC_WKT->CTRL = 1<<0;               // WKT_CTRL_CLKSEL
  NVIC_EnableIRQ(WKT_IRQn);

  // power-down setup (can't use deep power down, loses I/O pin state)
  LPC_SYSCON->STARTERP1 = 1<<15;      // wake up from alarm/wake timer
  LPC_SYSCON->PDAWAKECFG |= 1<<3;     // don't enable BOD on wakeup
  SCB->SCR = 1<<2;                    // enable SLEEPDEEP mode
  LPC_PMU->DPDCTRL = (1<<2) | (1<<1); // LPOSCEN, no wakepad
  LPC_PMU->PCON = 2;                  // use normal power-down mode

  LPC_SWM->PINENABLE0 = ~0;           // disable all special pin functions
  LPC_GPIO_PORT->DIR[0] |= 1<<1;      // pio1 is an output

  sleep(100); // sleep 10 ms to let the radio start up

  // SPI0 pin configuration
  // lpc810: sck=3p3, ssel=4p2, miso=2p4, mosi=5p1
  LPC_SWM->PINASSIGN[3] = 0x03FFFFFF; // sck  -    -    -
  LPC_SWM->PINASSIGN[4] = 0xFF040205; // -    nss  miso mosi

  // initialise the radio and put it into idle mode asap
  rf.init(61, 42, 8686);              // node 61, group 42, 868.6 MHz
  rf.sleep();

  // configure the radio a bit more
  rf.txPower(18); // 0 = min .. 31 = max

  // this data structure will be sent as packet
  static struct {
    uint32_t uniqId;                  // "fairly unique" for each LPC chip
    uint8_t nodeType :6;              // this is an MPS, so type = 1
    uint8_t seqNum :2;                // this is incremented after each tx
  } payload = { 0, 1, 0 };

  // get the 16-byte hardware id using the LPC's built-in IAP code in ROM
  uint32_t cmd = IAP_READ_UID_CMD, result[5];
  iap_entry(&cmd, result);
  // xor the 16-byte hardware id to turn it into a 32-bit number
  payload.uniqId = result[1] ^ result[2] ^ result[3] ^ result[4];

  sleep(10000); // sleep 1 sec before entering the main loop

  while (true) {
    // send out one packet and go back to sleep
    rf.send(0, &payload, 5);  // not "sizeof payload", which would be 8 !
    rf.sleep();

    ++payload.seqNum;

    sleep(10000); // sleep 1 sec
  }
}

extern "C" void WKT_IRQHandler () {
  LPC_WKT->CTRL = LPC_WKT->CTRL;      // clear the alarm interrupt
}
