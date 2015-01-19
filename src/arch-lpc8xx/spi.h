#include "LPC8xx.h"

#define SPI_CFG_ENABLE          (1<<0)
#define SPI_CFG_MASTER          (1<<2)

#define SPI_TXDATCTL_EOT        (1<<20)
#define SPI_TXDATCTL_FSIZE(s)   ((s)<<24)

#define SPI_STAT_RXRDY          (1<<0)

class SpiDevice {
public:
    void init () {
        LPC_SYSCON->SYSAHBCLKCTRL |= (1<<11);   // enable SPI clock
        LPC_SYSCON->PRESETCTRL &= ~(1<<0);      // reset
        LPC_SYSCON->PRESETCTRL |= (1<<0);       // release

        // 10 MHz, i.e. 30 MHz / 3 (or 4 MHz if clock is still at 12 MHz)
        LPC_SPI0->DIV = 2;
        LPC_SPI0->DLY = 0;

        LPC_SPI0->CFG = SPI_CFG_MASTER;
        LPC_SPI0->CFG |= SPI_CFG_ENABLE;
    }

    uint8_t rwReg (uint8_t cmd, uint8_t val) {
        LPC_SPI0->TXDATCTL =
			SPI_TXDATCTL_FSIZE(16-1) | SPI_TXDATCTL_EOT | (cmd << 8) | val;
        while ((LPC_SPI0->STAT & SPI_STAT_RXRDY) == 0)
            ;
        return LPC_SPI0->RXDAT;
    }
};
