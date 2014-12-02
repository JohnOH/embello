// Use comparator as 5-bit ADC, and adjust the LED blink rate accordingly.
// See http://jeelabs.org/2014/12/03/garage-parking-aid/

#include "LPC8xx.h"

// waste some time by doing nothing for a while
void delay (int count) {
    while (--count >= 0)
        __ASM(""); // twiddle thumbs
}

// setup the analog(ue) comparator, using the ladder on + and pin PIO0_1 on -
void analogSetup () {
    LPC_SYSCON->PDRUNCFG &= ~(1<<15);               // power up comparator
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<18) | (1<<19); // ACMP & IOCON clocks
    LPC_SYSCON->PRESETCTRL &= ~(1<<12);             // reset comparator
    LPC_SYSCON->PRESETCTRL |= (1<<12);              // release comparator

    // CLKIN has to be disabled, this is the default on power up
    LPC_SWM->PINENABLE0 &= ~(1<<1); // enable ACMP_I2 on PIO0_1

    // connect ACMP_I2 to CMP-, 20 mV hysteresis
    LPC_CMP->CTRL = (2<<11) | (3<<25);

    // disable pull-up, otherwise the results will be skewed
    LPC_IOCON->PIO0_1 &= ~(3<<3);
}

// measure the voltage on PIO0_1, returns a value from 0 to 32
// the steps will be roughly 0.1V apart, from 0 to 3.3V
int analogMeasure () {
    int i;
    for (i = 0; i < 32; ++i) {
        LPC_CMP->LAD = (i << 1) | 1;    // use ladder tap i
        delay(100);                     // approx 50 us settling delay
        if (LPC_CMP->CTRL & (1<<21))    // if COMPSTAT bit is set
            break;
    }
    return i;
}

int main () {
    LPC_GPIO_PORT->DIR0 |= 1<<4;        // make GPIO 4 an output pin

    analogSetup();

    // adjust the blink time as a suitable function of the measured value
    while (true) {
        int v = analogMeasure();        // returns 0..32

        // reversed, third order, and scaled so both limits are reasonable
        v = 32 - v;                     // 0 .. 32
        v = v * v * v;                  // 0 .. 32,768
        v *= 64;                        // 0 .. 2,097,152
        v += 100000;                    // 100,000 .. 2,197,152

        delay(v);                       // approx 50 ms .. 1 s
        LPC_GPIO_PORT->NOT0 = 1<<4;     // toggle LED
    }
}
