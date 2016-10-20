// Simple LED blink demo.
//
// See also https://github.com/libopencm3/libopencm3-examples/blob/master/
//                      examples/stm32/f1/stm32-h103/miniblink/miniblink.c

#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>

int main (void) {
    rcc_periph_clock_enable(RCC_GPIOA);

    gpio_mode_setup(GPIOA, GPIO_MODE_OUTPUT, GPIO_PUPD_NONE, GPIO15);

    for (;;) {
        for (int i = 0; i < 100000; ++i)
            __asm("");

        gpio_toggle(GPIOA, GPIO15);
    }

    return 0;
}
