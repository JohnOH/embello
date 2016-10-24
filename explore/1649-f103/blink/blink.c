// Simple LED blink demo.
//
// See also https://github.com/libopencm3/libopencm3-examples/blob/master/
//                      examples/stm32/f1/stm32-h103/miniblink/miniblink.c

#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/gpio.h>

int main (void) {
    rcc_periph_clock_enable(RCC_GPIOA);
    rcc_periph_clock_enable(RCC_GPIOC);

    gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_2_MHZ,
                    GPIO_CNF_OUTPUT_PUSHPULL, GPIO1);
    gpio_set_mode(GPIOC, GPIO_MODE_OUTPUT_2_MHZ,
                    GPIO_CNF_OUTPUT_PUSHPULL, GPIO13);

    for (;;) {
        for (int i = 0; i < 1000000; ++i)
            __asm("");

        gpio_toggle(GPIOA, GPIO1);
        gpio_toggle(GPIOC, GPIO13);
    }

    return 0;
}
