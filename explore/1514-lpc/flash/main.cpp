// Interface to an SPI-connected 8 MB dataflash memory chip.

#include "sys.h"
#include "spi_flash.h"

SpiFlash<SpiDev1> spif;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[flash]\n");

  LPC_SWM->PINENABLE0 |= 3<<2;          // disable SWCLK/SWDIO
  LPC_SWM->PINASSIGN[4] = 0x02FFFFFF;   // sck  -    -    -
  LPC_SWM->PINASSIGN[5] = 0xFF0D0703;   // -    nss  miso mosi
  // LPC_IOCON->PIO0[IOCON_PIO7] = 0x80;   // disable pull-up
  // LPC_IOCON->PIO0[IOCON_PIO7] = 0x98;   // repeater mode
  spif.init();

  printf("0x%x\n", spif.identify());

  printf("e");
  for (int n = 0; n < 10; ++n) {
    spif.eraseSector(n << 12);
    printf(" %u", (unsigned) tick.millis);
  }
  printf("\n");

  static uint8_t buf [256];

  printf("p");
  for (int n = 0; n < 10; ++n) {
    buf[0] = 11 * n;
    buf[1] = 22 * n;
    buf[2] = 33 * n;
    spif.program(n << 8, buf, 3);
    printf(" %u", (unsigned) tick.millis);
  }
  printf("\n");

  for (int i = 0; i < 3; ++i) {
    printf("\n");
    for (int n = 0; n < 11; ++n) {
      spif.read(n << 8, buf, 3);
      printf("#%d: %d,%d,%d @ %u ms\n", n, buf[0], buf[1], buf[2],
                                    (unsigned) tick.millis);
    }
  }
}
