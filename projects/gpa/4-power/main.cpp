// Report distance over serial, power down IR sensor in between.
// See http://jeelabs.org/2014/12/03/garage-parking-aid/

#include "stdio.h"
#include "serial.h"

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

    LPC_SWM->PINENABLE0 |= (2<<2);      // disable both SWD pins
    LPC_GPIO_PORT->DIR0 |= (1<<3);      // make PIO0_3 an output
    LPC_GPIO_PORT->SET0 = (1<<3);       // power down the IR sensor
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

// power up the sensor, perform one measurement, and power it down again
int getDistance () {
    LPC_GPIO_PORT->CLR0 = (1<<3);       // power up the IR sensor
    delay(50);                          // give it time to settle
    int v = analogMeasure();            // measure the voltage
    LPC_GPIO_PORT->SET0 = (1<<3);       // power down the IR sensor
    return v;
}

int main () {
    // send UART output to GPIO 4, running at 115200 baud
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL;
    serial.init(LPC_USART0, 115200);

    printf("\n[gpa/4-power]\n");

    analogSetup();

    // measure and report the value about twice a second
    while (true) {
        printf("analog = %d\n", getDistance());
        delay(1000000);
    }
}
