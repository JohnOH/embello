// Wait for hall sensor to trigger, then flash some LEDs

#include "embello.h"

const int LED1 = 1;
const int LED2 = 6;
const int LED3 = 8;
const int LED4 = 9;
const int LED5 = 14;
const int LED6 = 15;
const int LED7 = 16;
const int LED8 = 17;

const int ALL_LEDS = (1<<LED1) | (1<<LED2) | (1<<LED3) | (1<<LED4) |
                     (1<<LED5) | (1<<LED6) | (1<<LED7) | (1<<LED8);

const int HPOWER = 7;
const int HSENSE = 13;

int main () {
    tick.init(1000);
    serial.init(115200);

    printf("\n[fpd]\n");

    // all eight LED pins are outputs and start high (off)
    LPC_GPIO_PORT->SET[0] = ALL_LEDS;
    LPC_GPIO_PORT->DIR[0] = ALL_LEDS;

    // turn on power to the hall sensor,
    LPC_GPIO_PORT->SET[0] = (1<<HPOWER);
    LPC_GPIO_PORT->DIR[0] |= (1<<HPOWER);

    while (true) {
        // wait while the hall sensor is high (inactive)
        while (LPC_GPIO_PORT->B[0][HSENSE])
            ;
        printf("%u\n", tick.millis);

        // 10 brief demo blinks
        for (int i = 0; i < 10; ++i) {
            LPC_GPIO_PORT->CLR[0] = ALL_LEDS;
            tick.delay(5);
            LPC_GPIO_PORT->SET[0] = ALL_LEDS;
            tick.delay(50);
        }

        // insert a brief pause
        tick.delay(500);
    }
}
