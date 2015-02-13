#include "LPC8xx.h"

#define SPI_CFG_ENABLE          (1<<0)
#define SPI_CFG_MASTER          (1<<2)

#define SPI_TXDATCTL_EOT        (1<<20)
#define SPI_TXDATCTL_FSIZE(s)   ((s)<<24)

#define SPI_STAT_RXRDY          (1<<0)

template< int N >
class SpiDev {
public:
    static void master (int div) {
        LPC_SYSCON->SYSAHBCLKCTRL |= (1<<11);   // enable SPI clock
        LPC_SYSCON->PRESETCTRL &= ~(1<<0);      // reset
        LPC_SYSCON->PRESETCTRL |= (1<<0);       // release

        addr()->DIV = div-1;
        addr()->DLY = 0;

        addr()->CFG = SPI_CFG_MASTER;
        addr()->CFG |= SPI_CFG_ENABLE;
    }

    static uint8_t rwReg (uint8_t cmd, uint8_t val) {
        addr()->TXDATCTL =
			SPI_TXDATCTL_FSIZE(16-1) | SPI_TXDATCTL_EOT | (cmd << 8) | val;
        while ((addr()->STAT & SPI_STAT_RXRDY) == 0)
            ;
        return addr()->RXDAT;
    }

protected:
    static LPC_SPI_TypeDef* addr () { return N == 0 ? LPC_SPI0 : LPC_SPI1; }
};

typedef SpiDev<0> SpiDev0;
typedef SpiDev<1> SpiDev1;
