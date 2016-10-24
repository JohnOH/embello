// Report distance on serial and then LED, power down IR sensor in between.
// See http://jeelabs.org/2014/12/03/garage-parking-aid/

#include "stdio.h"
#include "serial.h"

extern "C" void SysTick_Handler () {
    // the only effect is to generate an interrupt, no work is done here
}

void delay (int millis) {
    while (--millis >= 0)
        __WFI(); // wait for the next SysTick interrupt
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

    LPC_SWM->PINENABLE0 |= (3<<2);      // disable both SWD pins
    LPC_GPIO_PORT->DIR0 |= (1<<3);      // make PIO0_3 an output
    LPC_GPIO_PORT->SET0 = (1<<3);       // power down the IR sensor
}

// measure the voltage on PIO0_1, returns a value from 0 to 32
// the steps will be roughly 0.1V apart, from 0 to 3.3V
int analogMeasure () {
    int i;
    for (i = 0; i < 32; ++i) {
        LPC_CMP->LAD = (i << 1) | 1;                // use ladder tap i
        for (int i = 0; i < 500; ++i) __ASM("");    // brief settling delay
        if (LPC_CMP->CTRL & (1<<21))                // if COMPSTAT bit is set
            break;                                  // ... we're done
    }
    return i;
}

// power up the sensor, perform one measurement, and power it down again
int getDistance () {
    LPC_GPIO_PORT->CLR0 = (1<<3);       // power up the IR sensor
    delay(50);                          // give it 50 ms to settle
    int v = analogMeasure();            // measure the voltage
    LPC_GPIO_PORT->SET0 = (1<<3);       // power down the IR sensor
    return v;
}

int main () {
    // send UART output to GPIO 4, running at 115200 baud
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL;
    serial.init(LPC_USART0, 115200);

    printf("\n[gpa/4-power]\n");

    SysTick_Config(12000000/1000);      // 1000 Hz

    analogSetup();

    // measure and report the value twice a second, but only 10 times
    for (int i = 0; i < 10; ++i) {
        printf("analog = %d\n", getDistance());
        delay(500);
    }

    serial.deInit();                    // terminate serial port use
    LPC_SWM->PINASSIGN0 = 0xFFFFFFFFUL;
    LPC_GPIO_PORT->DIR0 |= 1<<4;        // turn GPIO 4 into an output pin

    // adjust the blink time as a suitable function of the measured value
    while (true) {
        int v = getDistance();          // returns 0..32

        // reversed, third power, scaled, and shifted to get reasonable limits
        v = 32 - v;                     // 0 .. 32
        v = v * v * v;                  // 0 .. 32,768
        v /= 16;                        // 0 .. 2,048
        v += 50;                        // 50 .. 2,098

        delay(v);                       // approx 50 ms .. 2 s range
        LPC_GPIO_PORT->NOT0 = 1<<4;     // toggle LED
    }
}
