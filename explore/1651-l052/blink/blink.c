// Simple LED blink demo.
//
// See also https://github.com/libopencm3/libopencm3-examples/blob/master/
//                      examples/stm32/l0/stm32l0538-disco/miniblink/miniblink.c

#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>

int main (void) {
    rcc_periph_clock_enable(RCC_GPIOA);

    gpio_mode_setup(GPIOB, GPIO_MODE_OUTPUT, GPIO_PUPD_NONE, GPIO5);

    for (;;) {
        for (int i = 0; i < 100000; ++i)
            __asm("");

        gpio_toggle(GPIOB, GPIO5);
    }

    return 0;
}
