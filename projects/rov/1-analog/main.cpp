// Simple Reflow Oven controller, using the Thermo Plug w/ AD597.

#include "embello.h"
#include "adc_sct.h"

AdcSct<1> adc;  // use ACMP_I1 on 0p8

extern "C" void SCT_IRQHandler () { adc.sctIrqHandler(); }

static int getTemp () {
  int accum = 0;
  for (int i = 0; i < 10; ++i)
    accum += adc.result();
  return accum / 35;
}

static void waitForTemp (const char* tag, int targetTemp, int dwell) {
  printf("%s\n", tag);

  LPC_GPIO_PORT->B[0][3] = 1;         // turn the heater on

  while (true) {
    int temp = getTemp();
    printf("  %d C\n", temp);
    if (temp >= targetTemp)
      break;
    tick.delay(1000);
  }

  LPC_GPIO_PORT->B[0][3] = 0;         // turn the heater off

  printf("    wait %d s\n", dwell);
  tick.delay(1000 * dwell);
}

int main () {
  tick.init(1000);
  serial.init(115200);
  printf("\n[rovtest]\n");

  LPC_SWM->PINENABLE0 |= (3<<2);      // disable SWDIO and SWCLK
  LPC_SWM->PINASSIGN[8] = 0xFFFF02FF; // enable ACMP_O on PIO_2
  LPC_SWM->PINASSIGN[5] = 0x02FFFFFF; // enable CTIN_0 on PIO_2
  LPC_GPIO_PORT->DIR[0] |= (1<<3);    // set 3p3 as output, heater

  adc.init();
  tick.delay(100);
  adc.result();

  waitForTemp("start", 50, 1);
  waitForTemp("dwell", 150, 20);
  waitForTemp("ramp", 200, 3);
  waitForTemp("peak", 250, 10);
  printf("done!\n");

  tick.delay(3000);

  SCB->SCR |= 1<<2;                   // enable SLEEPDEEP mode
  LPC_PMU->DPDCTRL |= (1<<2)|(1<<3);  // LPOSCEN and LPOSCDPDEN
  LPC_PMU->PCON = 3;                  // enter deep power-down mode
  __WFI();                            // wait for interrupt, powers down
}
