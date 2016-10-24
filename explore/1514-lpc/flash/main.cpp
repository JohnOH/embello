// Interface to an SPI-connected 8 MB dataflash memory chip.

#include "embello.h"
#include "spi_flash.h"

SpiFlash<SpiDev0> spif;

int main () {
  tick.init(1000);
  serial.init(115200);

  printf("\n[flash]\n");

  LPC_SWM->PINENABLE0 |= (3<<2);        // disable SWCLK/SWDIO
  // lpc810: sck 1, ssel 5, miso 7, mosi 3
  LPC_SWM->PINENABLE0 |= (1<<6);        // also needs RESET pin (p5)
  LPC_SWM->PINASSIGN[3] = 0x01FFFFFF;   // sck  -    -    -
  LPC_SWM->PINASSIGN[4] = 0xFF050302;   // -    nss  miso mosi
  // jnp v0.2: sck 2, ssel 13, miso 7, mosi 3
  // LPC_SWM->PINASSIGN[3] = 0x02FFFFFF;   // sck  -    -    -
  // LPC_SWM->PINASSIGN[4] = 0xFF0D0703;   // -    nss  miso mosi
  // eb20soic A: sck 7, ssel 2, miso 3, mosi 6
  // LPC_SWM->PINASSIGN[3] = 0x07FFFFFF;   // sck  -    -    -
  // LPC_SWM->PINASSIGN[4] = 0xFF020306;   // -    nss  miso mosi
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
    spif.program(n << 8, buf, sizeof buf);
    printf(" %u", (unsigned) tick.millis);
  }
  printf("\n");

  for (int i = 0; i < 3; ++i) {
    printf("\n");
    for (int n = 0; n < 11; ++n) {
      spif.read(n << 8, buf, sizeof buf);
      printf("#%d: %d,%d,%d @ %u ms\n", n, buf[0], buf[1], buf[2],
                                    (unsigned) tick.millis);
    }
  }

  // generate constant readout signals for scope / logic analyser use
  while (true)
    spif.read(0x1000, buf, sizeof buf);
}
