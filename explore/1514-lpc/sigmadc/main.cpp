// Sigma-delta ADC based on NXP's AppNote 11329.

#include "sys.h"

#define CONFIG_SD_ADC_PRESCALER   6    // 12 MHz => max rate is 2 MHz
#define CONFIG_WINDOW_SIZE        1024
#define CONFIG_VLADDER_PRESCALER  14
#define CONFIG_ENABLE_DEBUG_PIN   13

const int FEEDBACK_PIN = 7;
const int ADC_CMP_PIN = 1;  // ACMP_I2

static void sct_init () {
	/* Enable register access to the SCT */
	LPC_SYSCON->SYSAHBCLKCTRL |= (1<<8);

	/* Clear SCT reset */
	LPC_SYSCON->PRESETCTRL |= (1<<8);

	/* Set SCT:
	 * - As two 16-bit counters
	 * - Use bus clock as the SCT and prescaler clock
	 * - Allows reload the match registers
	 * - Sync CTIN_0 with bus clock before triggering any event
	 */
	LPC_SCT->CONFIG = 0x200;

	/* Set averaging window size. Reset the HIGH and LOW counter when maximum
	 * window size reached, and generate interrupt to save the LOW counter value
	 *
	 * This maximum counting is used as max window size
	 */
	LPC_SCT->REGMODE_H = 0x0000;          /*< Use HIGH counter register 0 as
	                                          Match */
	LPC_SCT->MATCH[0].H     = CONFIG_WINDOW_SIZE - 1;
	LPC_SCT->MATCHREL[0].H  = CONFIG_WINDOW_SIZE - 1;
	LPC_SCT->EV[0].CTRL  = 0x00001010; /*< Set event when MR0 = COUNT_H =
		                                          max window size */
	LPC_SCT->EV[0].STATE = 0x00000003; /*< Trigger event at any state */
	LPC_SCT->LIMIT_H = 0x0001;            /*< Reset HIGH counter when Event #0,
	                                          triggered */

	/* Set capture event to capture the analog comparator output. The capture
	 * event will be triggered if the analog output is toggled
	 */
	LPC_SCT->EV[1].CTRL  = 0x00002400; /*< Set event when CTIN_0 is rising
	                                       */
	LPC_SCT->EV[1].STATE = 0x00000003; /*< Trigger event at any state */
	LPC_SCT->START_L = 0x0002;            /*< Start counting on LOW counter when
	                                          Event #1 triggered */
#ifdef CONFIG_ENABLE_DEBUG_PIN
  LPC_SCT->OUT[0].SET = 2;
#endif
	LPC_SCT->EV[2].CTRL  = 0x00002800; /*< Set event when CTIN_0 is falling
	                                       */
	LPC_SCT->EV[2].STATE = 0x00000003; /*< Trigger event at any state */
	LPC_SCT->STOP_L = 0x0004;            /*< Stop counting on LOW counter when
	                                          Event #2 triggered */
#ifdef CONFIG_ENABLE_DEBUG_PIN
	LPC_SCT->OUT[0].CLR = 4;
#endif
	/* Trigger interrupt when Event #0 triggered */
	LPC_SCT->EVEN =    0x00000001;

	/* Start the HIGH counter:
	 * - Counting up
	 * - Single direction
	 * - Prescaler = bus_clk / CONFIG_SD_ADC_PRESCALER
	 * - Reset the HIGH counter
	 */
	LPC_SCT->CTRL_H = ((CONFIG_SD_ADC_PRESCALER - 1) << 5) | 0x08;

	/* Stop the LOW counter:
	 * - Counting up
	 * - Single direction
	 * - Prescaler = bus_clk / CONFIG_SD_ADC_PRESCALER
	 * - Reset the LOW counter
	 */
	LPC_SCT->CTRL_L = ((CONFIG_SD_ADC_PRESCALER - 1) << 5) | 0x0A;
}

volatile uint16_t adc_result;

int main () {
  LPC_SYSCON->PRESETCTRL = 1<<11; // all except flash!
  LPC_SYSCON->PRESETCTRL = ~0; // all!
  LPC_SYSCON->SYSAHBCLKCTRL = ~0; // all!

  tick.init(1000);
  serial.init(115200);

  printf("\n[sigmadc]\n");

  LPC_IOCON->PIO0[FEEDBACK_PIN] = 0x80; // disable pull-up
	LPC_IOCON->PIO0[ADC_CMP_PIN] = 0x80; // disable pull-up
  LPC_SWM->PINENABLE0 &= ~(1<<1); // enable ACMP_I2
  LPC_SWM->PINASSIGN[8] &= ~(0xFF<<8);
	LPC_SWM->PINASSIGN[8] |= (FEEDBACK_PIN<<8); // enable ACMP_O
  LPC_SWM->PINASSIGN[5] &= ~(0xFF<<24);
	LPC_SWM->PINASSIGN[5] |= (FEEDBACK_PIN<<24);  // enable CTIN_0
#ifdef CONFIG_ENABLE_DEBUG_PIN
	// LPC_IOCON->PIO0[CONFIG_ENABLE_DEBUG_PIN] = 0x80; // disable pull-up
  LPC_SWM->PINASSIGN[6] &= ~(0xFF << 24);
	LPC_SWM->PINASSIGN[6] |= (CONFIG_ENABLE_DEBUG_PIN << 24);  // enable CTOUT_0
#endif
  LPC_SYSCON->PDRUNCFG &= ~(1<<15); // power up comparator
  LPC_SYSCON->PRESETCTRL |= (1<<12); // clear comparator reset
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<19); // enable access to the comparator
  sct_init();
  LPC_CMP->LAD = (CONFIG_VLADDER_PRESCALER << 1) | 1;
	for (volatile uint32_t delay = 0; delay < 250; ++delay)
    ; // delay is needed to stabilize the VLadder output
  LPC_CMP->CTRL = 0;
	// LPC_CMP->CTRL |= (vp_channel<<8) | (vn_channel<<11);
	LPC_CMP->CTRL |= (0<<8) | (2<<11);
	// LPC_CMP->CTRL |= (1<<6); // jcw
	// LPC_SYSCON->SYSAHBCLKCTRL &= ~(1<<19); // disable to save power
  NVIC_EnableIRQ(SCT_IRQn);

  while (true) {
    tick.delay(500);
    printf("%u\n", adc_result);
  }
}

extern "C" void SCT_IRQHandler () {
  uint32_t event_flag = LPC_SCT->EVFLAG;
  static uint32_t old_capture_val;
  uint32_t capture_val;

  /* Reject interrupt from events that are not belonging to this module */
  if ((event_flag & 1) == 0) {
    return;
  }

  capture_val = LPC_SCT->COUNT_L;
  // printf("!%d",capture_val);
  adc_result = capture_val - old_capture_val;
  old_capture_val = capture_val;

  // SDADC_USER_IRQ(adc_result);

  LPC_SCT->EVFLAG = 1;    /*< Clear the IRQ flag */
}
