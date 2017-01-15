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
        LPC_SWM->PINASSIGN[0] = 0xFFFF0004;
        uart0Init(baud);
    }
};

class Analog {
public:
    void init () {
        LPC_SYSCON->PDRUNCFG &= ~(1<<4);            // power up ADC
        LPC_SYSCON->SYSAHBCLKCTRL |= (1<<24);       // enable ADC clock
        LPC_ADC->CTRL = (1<<30) | 2;                // start calib at 500 kHz
        while (LPC_ADC->CTRL & (1<<30))
            ;
        LPC_ADC->CTRL = (1<<10); // set adc clock to max speed, low-power mode
    }

    int measure(int chan) {
        LPC_SWM->PINENABLE0 &= ~(1<<(13+chan));     // enable the analog pin
        LPC_ADC->SEQ_CTRL[ADC_SEQA_IDX] = (1<<chan);
        LPC_ADC->SEQ_CTRL[ADC_SEQA_IDX] |= (1<<18) | (1<<31);
        LPC_ADC->SEQ_CTRL[ADC_SEQA_IDX] |= (1<<26); // start the ADC conversion
        int data;
        for (;;) {
            data = LPC_ADC->SEQ_GDAT[ADC_SEQA_IDX];
            if (data < 0)
                break;
        }
        LPC_SWM->PINENABLE0 |= (1<<(13+chan));      // disable the analog pin
        return (uint16_t) data >> 4;
    }
};

template < int N >
class Pin {
public:
    operator int ()             { return LPC_GPIO_PORT->B[0][N]; }
    void operator= (int value)  { LPC_GPIO_PORT->B[0][N] = value != 0; }
    void setInput ()            { LPC_GPIO_PORT->DIR[0] &= ~(1<<N); }
    void setOutput ()           { LPC_GPIO_PORT->DIR[0] |= 1<<N; }
    void toggle ()              { LPC_GPIO_PORT->NOT[0] = 1<<N; }
    int pin ()                  { return N; }
};

extern Tick tick;
extern Serial serial;
extern Analog analog;
