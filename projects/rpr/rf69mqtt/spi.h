template< int N, int S =0 >
class SpiDev {
  public:
    static void master (int div) {
    }

    static void enable () {
      // careful: TXCTL needs to be set as 8, TXDATCTL would be 8-1 (crazy!)
      ///addr()->TXCTRL = SPI_TXCTL_FLEN(8) | SPI_TXCTL_DEASSERTNUM_SSEL(S);
    }

    static void disable () {
      ///addr()->STAT |= SPI_STAT_EOT;
    }

    static uint8_t transfer (uint8_t val) {
      ///addr()->TXDAT = val;
      ///while ((addr()->STAT & SPI_STAT_RXRDY) == 0)
      ///  ;
      ///return addr()->RXDAT;
      return 0;
    }

    static uint8_t rwReg (uint8_t cmd, uint8_t val) {
      uint8_t data[2] = { cmd, val };
      if (wiringPiSPIDataRW (N, data, 2) == -1) {
        printf("SPI error\n");
        return 0;
      }
      return data[1];
    }

    //static LPC_SPI_T* addr () { return N == 0 ? LPC_SPI0 : LPC_SPI1; }
};

typedef SpiDev<0> SpiDev0;
typedef SpiDev<1> SpiDev1;
