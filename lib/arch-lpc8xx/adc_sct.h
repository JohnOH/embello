// Delta-sigma ADC implementation based on NXP's AppNote 11329.

template < int ADC_CMP_NUM, int AVG_FACTOR =1 >
class AdcSct {
  enum { SD_ADC_PRESCALER = 30 }; // 400 kHz => max rate is 2 MHz
  enum { WINDOW_SIZE = 1024 };    // 10-bit oversampling
  enum { VLADDER_TAP = 17 };      // compare at Vcc/31*17

public:
  static void init () {
    LPC_SWM->PINENABLE0 &= ~(1<<(ADC_CMP_NUM-1)); // enable ACMP
    LPC_SYSCON->PDRUNCFG &= ~(1<<15); // power up comparator
    LPC_SYSCON->PRESETCTRL |= (1<<12); // clear comparator reset
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<19); // enable ACMP access
    sct_init();
    LPC_CMP->LAD = (VLADDER_TAP << 1) | 1;
    // for (volatile uint32_t delay = 0; delay < 250; ++delay) ;
    LPC_CMP->CTRL = (0<<8) | (ADC_CMP_NUM<<11);
    NVIC_EnableIRQ(SCT_IRQn);
  }

  static bool sctIrqHandler () {
    static uint32_t old_capture_val;
    if (LPC_SCT->EVFLAG & 1) {
      uint32_t capture_val = LPC_SCT->COUNT_L;
      uint16_t val = WINDOW_SIZE - (capture_val - old_capture_val);
      old_capture_val = capture_val;
      // track results as running average, if AVG_FACTOR > 1
      result() = (result() * (AVG_FACTOR-1)) / AVG_FACTOR + val;
      LPC_SCT->EVFLAG = 1;    /*< Clear the IRQ flag */
      return true;
    }
    return false;
  }

  static volatile uint32_t& result () {
    static volatile uint32_t adc_result;
    return adc_result;
  }

private:
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
};
