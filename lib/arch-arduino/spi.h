// SPI setup for ATmega (JeeNode) and ATtiny (JeeNode Micro)
// ATtiny thx to @woelfs, see http://jeelabs.net/boards/11/topics/6493

template< int N>
class SpiDev {
  static uint8_t spiTransferByte (uint8_t out) {
#ifdef SPCR
    SPDR = out;
    while ((SPSR & (1<<SPIF)) == 0)
      ;
    return SPDR;
#else
      // ATtiny
      USIDR = out;
      byte v1 = bit(USIWM0) | bit(USITC);
      byte v2 = bit(USIWM0) | bit(USITC) | bit(USICLK);
      for (uint8_t i = 0; i < 8; ++i) {
        USICR = v1;
        USICR = v2;
    }
    return USIDR;
#endif
  }

public:
  static void master (int div) {
    digitalWrite(N, 1);
    pinMode(N, OUTPUT);

#ifdef SPCR
    pinMode(10, OUTPUT);
    pinMode(11, OUTPUT);
    pinMode(12, INPUT);
    pinMode(13, OUTPUT);

    SPCR = _BV(SPE) | _BV(MSTR);
    SPSR |= _BV(SPI2X);
#else
    pinMode(1, OUTPUT); // SS
    pinMode(4, INPUT);  // MISO 7
    pinMode(5, OUTPUT); // MOSI 8
    pinMode(6, OUTPUT); // SCK 9

    USICR = bit(USIWM0);
#endif
  }

  static uint8_t rwReg (uint8_t cmd, uint8_t val) {
    digitalWrite(N, 0);
    spiTransferByte(cmd);
    uint8_t in = spiTransferByte(val);
    digitalWrite(N, 1);
    return in;
  }
};

typedef SpiDev<10> SpiDev10;
