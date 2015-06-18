// Wait for hall sensor to trigger, then flash some LEDs

const int LED1 = 1;
const int LED2 = 6;
const int LED3 = 8;

const int HPOWER = 7;
const int HSENSE = 13;

#include "sys.h"

int main () {
    tick.init(1000);
    serial.init(115200);

    printf("\n[fpd]\n");

    LPC_GPIO_PORT->SET[0] = (1<<LED1) | (1<<LED2) | (1<<LED3);
    LPC_GPIO_PORT->DIR[0] |= (1<<LED1) | (1<<LED2) | (1<<LED3);

    LPC_GPIO_PORT->SET[0] = (1<<HPOWER);
    LPC_GPIO_PORT->DIR[0] |= (1<<HPOWER);

    while (true) {
        while (LPC_GPIO_PORT->B[0][HSENSE])
            ;

        printf("%u\n", tick.millis);

        for (int i = 0; i < 10; ++i) {
            LPC_GPIO_PORT->NOT[0] = (1<<LED1) | (1<<LED2) | (1<<LED3);
            tick.delay(5);
            LPC_GPIO_PORT->NOT[0] = (1<<LED1) | (1<<LED2) | (1<<LED3);
            tick.delay(50);
        }

        tick.delay(500);
    }
}
