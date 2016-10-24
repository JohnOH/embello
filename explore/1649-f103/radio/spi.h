// SPI setup for STM32L0xx with libopencm3

#include <libopencm3/stm32/rcc.h>
#include <libopencm3/stm32/spi.h>

class SpiDev {
    static uint8_t spiTransferByte (uint8_t out) {
        return spi_xfer(SPI1, out);
    }

    public:
    static void master (int /*div*/) {
        rcc_periph_clock_enable(RCC_SPI1);

        gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_10_MHZ,
                GPIO_CNF_OUTPUT_PUSHPULL, GPIO4);
        gpio_set(GPIOA, GPIO4);

        gpio_set_mode(GPIOA, GPIO_MODE_OUTPUT_50_MHZ,
                GPIO_CNF_OUTPUT_ALTFN_PUSHPULL,
                GPIO_SPI1_SCK|GPIO_SPI1_MISO|GPIO_SPI1_MOSI);

        spi_set_master_mode(SPI1);
        spi_set_baudrate_prescaler(SPI1, SPI_CR1_BR_FPCLK_DIV_8);
        spi_enable_software_slave_management(SPI1);
        spi_set_nss_high(SPI1);
        spi_enable(SPI1);
    }

    static uint8_t rwReg (uint8_t cmd, uint8_t val) {
        gpio_clear(GPIOA, GPIO4);
        spiTransferByte(cmd);
        uint8_t in = spiTransferByte(val);
        gpio_set(GPIOA, GPIO4);
        return in;
    }
};
