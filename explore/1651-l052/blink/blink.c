// Simple LED blink demo for the JeeNode Zero.
//
// See also https://github.com/libopencm3/libopencm3-examples/blob/master/
//                      examples/stm32/l0/stm32l0538-disco/miniblink/miniblink.c

#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>

int main (void) {
    rcc_periph_clock_enable(RCC_GPIOA);
    rcc_periph_clock_enable(RCC_GPIOB);

    gpio_mode_setup(GPIOA, GPIO_MODE_OUTPUT, GPIO_PUPD_NONE, GPIO15);   // rev1
    gpio_mode_setup(GPIOB, GPIO_MODE_OUTPUT, GPIO_PUPD_NONE, GPIO5);    // rev3
    gpio_mode_setup(GPIOA, GPIO_MODE_OUTPUT, GPIO_PUPD_NONE, GPIO8);    // rev4

    for (;;) {
        for (int i = 0; i < 100000; ++i)
            __asm("");

        gpio_toggle(GPIOA, GPIO15); // rev1
        gpio_toggle(GPIOB, GPIO5);  // rev3
        gpio_toggle(GPIOA, GPIO8);  // rev4
    }

    return 0;
}
