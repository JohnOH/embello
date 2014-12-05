// Distance reporting for the GPA with power down and LED-off idling.
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

    printf("\n[gpa/5-idle]\n");

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

    // these variables must retain their value across the loop
    int avgTimes16, changed;

    // adjust the blink time as a suitable function of the measured value
    while (true) {
        int v = getDistance();          // returns 0..32

        if (v <= 6) {                   // if too far away: don't blink
            LPC_GPIO_PORT->SET0 = 1<<4; // turn LED off
            delay(1000);                // do nothing for one second
            continue;                   // loop to get next readout
        }

        // The following logic implements an auto power-down mode when the
        // measured distance does not change much for 32 times in succession.
        // Uses a moving average and knows about the last 32 distance changes.

        // calculate a moving average to slowly track measurement changes
        // each loop adds 1/16th of v to 15/16th of the average so far
        avgTimes16 = v + (15 * avgTimes16) / 16;

        // use the 32 bits in an int as history to remember major changes
        // each loop shifts out one bit and brings in a new one
        changed <<= 1;

        // a "major change" is defined as being more than one off the average
        if (v < avgTimes16/16 - 1 || v > avgTimes16/16 + 1)
            changed |= 1;

        // once idling, we can wait until the car is completely out of range,
        // since we don't need the parking aid to help us *leave* the garage!
        if (changed == 0) { // if we saw no major change 32 times in a row
            LPC_GPIO_PORT->SET0 = 1<<4; // turn LED off
            do {
                delay(1000);            // do nothing for one second
                v = getDistance();
            } while (v > 6);            // loop until object is too far
            changed = 1;                // mark history as changed again
        }

        // reversed, third power, scaled, and shifted to get reasonable limits
        v = 32 - v;                     // 0 .. 32
        v = v * v * v;                  // 0 .. 32,768
        v /= 16;                        // 0 .. 2,048
        v += 50;                        // 50 .. 2,098

        delay(v);                       // approx 50 ms .. 2 s toggle rate
        LPC_GPIO_PORT->NOT0 = 1<<4;     // toggle LED
    }
}
