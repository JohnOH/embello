class SpiDev {
  public:
    static void master (int div) {
        pinMode(PA4, OUTPUT);
        SPI.begin();
    }

    static void enable () {
        digitalWrite(PA4, 0);
    }

    static void disable () {
        digitalWrite(PA4, 1);
    }

    static uint8_t transfer (uint8_t val) {
        return SPI.transfer(val);
    }

    static uint8_t rwReg (uint8_t cmd, uint8_t val) {
        enable();
        transfer(cmd);
        uint8_t result = transfer(val);
        disable();
        return result;
    }
};
