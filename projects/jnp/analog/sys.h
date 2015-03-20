#include "chip.h"
#include "uart.h"
#include <stdio.h>

class Tick {
public:
  static void init (int hz, bool fast =false) {
    SysTick_Config((fast ? 30000000 : 12000000) / hz);
  }

  static void delay (unsigned ms) {
    uint32_t start = millis;
    while (millis - start < ms)
      __WFI();
  }

  static volatile unsigned millis;
};

class Serial {
public:
  static void init (int baud) {
    LPC_SWM->PINASSIGN[0] = 0xFFFFFF04;
    uart0Init(baud);
  }
};

class Analog {
public:
  void init () {
    LPC_SYSCON->PDRUNCFG &= ~(1<<4); // power up ADC
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<24); // enable ADC clock
    LPC_ADC->CTRL = (1<<30) | 24; // start calibration at 500 kHz
    printf("a1\n");
    while (LPC_ADC->CTRL & (1<<30))
      ;
    printf("a2\n");
    LPC_ADC->CTRL = (1<<10) | 12; // set adc clock to 1 MHz, low-power mode
  }

  int measure(int chan) {
    LPC_SWM->PINENABLE0 &= ~(1<<(13+chan)); // enable the analog pin
    LPC_ADC->SEQ_CTRL[ADC_SEQA_IDX] = (1<<chan); // use seqA for given channel
    LPC_ADC->SEQ_CTRL[ADC_SEQA_IDX] |= (1<<18) | (1<<31); // TRIGPOL & SEQA_ENA
    LPC_ADC->SEQ_CTRL[ADC_SEQA_IDX] |= (1<<26); // start the ADC conversion
    for (;;) {
      // uint32_t data = LPC_ADC->SEQ_GDAT[ADC_SEQA_IDX];
      uint32_t data = LPC_ADC->DR[chan];
      if (data & (1<<31))
        return (uint16_t) data >> 4;
    }
  }
};

extern Tick tick;
extern Serial serial;
extern Analog analog;
