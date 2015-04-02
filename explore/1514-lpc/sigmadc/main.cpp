// Sigma-delta ADC based on NXP's AppNote 11329.

#include "sys.h"

#define SD_ADC_PRESCALER   120   // 100 Khz => max rate is 2 MHz
#define WINDOW_SIZE        1024
#define VLADDER_PRESCALER  17

#define FEEDBACK_PIN  7
#define ADC_CMP_PIN   1  // ACMP_I2

static void sct_init () {
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);
  LPC_SYSCON->PRESETCTRL |= (1<<8);
  LPC_SCT->CONFIG = 0x200;
  LPC_SCT->REGMODE_H = 0x0000;
  LPC_SCT->MATCH[0].H = LPC_SCT->MATCHREL[0].H = WINDOW_SIZE - 1;
  LPC_SCT->EV[0].CTRL = 0x00001010;
  LPC_SCT->EV[0].STATE = 0x00000003;
  LPC_SCT->LIMIT_H = 0x0001;
  LPC_SCT->EV[1].CTRL = 0x00002400;
  LPC_SCT->EV[1].STATE = 0x00000003;
  LPC_SCT->START_L = 0x0002;
  LPC_SCT->EV[2].CTRL = 0x00002800;
  LPC_SCT->EV[2].STATE = 0x00000003;
  LPC_SCT->STOP_L = 0x0004;
  LPC_SCT->EVEN = 0x00000001;
  LPC_SCT->CTRL_H = ((SD_ADC_PRESCALER - 1) << 5) | 0x08;
  LPC_SCT->CTRL_L = ((SD_ADC_PRESCALER - 1) << 5) | 0x0A;
}

volatile int adc_result;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[sigmadc]\n");

  LPC_IOCON->PIO0[ADC_CMP_PIN] = 0x80; // disable pull-up
  LPC_SWM->PINENABLE0 &= ~(1<<1); // enable ACMP_I2
  LPC_SWM->PINASSIGN[8] &= ~(0xFF<<8);
  LPC_SWM->PINASSIGN[8] |= (FEEDBACK_PIN<<8); // enable ACMP_O
  LPC_SWM->PINASSIGN[5] &= ~(0xFF<<24);
  LPC_SWM->PINASSIGN[5] |= (FEEDBACK_PIN<<24);  // enable CTIN_0
  LPC_SYSCON->PDRUNCFG &= ~(1<<15); // power up comparator
  LPC_SYSCON->PRESETCTRL |= (1<<12); // clear comparator reset
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<19); // enable access to the comparator

  sct_init();

  LPC_CMP->LAD = (VLADDER_PRESCALER << 1) | 1;
  // for (volatile uint32_t delay = 0; delay < 250; ++delay) ;
  // LPC_CMP->CTRL = (vp_channel<<8) | (vn_channel<<11);
  LPC_CMP->CTRL = (0<<8) | (2<<11);
  LPC_SYSCON->SYSAHBCLKCTRL &= ~(1<<19); // disable to save power
  NVIC_EnableIRQ(SCT_IRQn);

  while (true) {
    tick.delay(500);
    printf("%d\n", adc_result);
  }
}

static uint32_t old_capture_val;

extern "C" void SCT_IRQHandler () {
  if (LPC_SCT->EVFLAG & 1) {
    uint32_t capture_val = LPC_SCT->COUNT_L;
    uint16_t result = capture_val - old_capture_val;
    old_capture_val = capture_val;
    // adc_result = (15 * adc_result + result) / 16; // running / damped average
    adc_result = result;
    LPC_SCT->EVFLAG = 1;    /*< Clear the IRQ flag */
  }
}
