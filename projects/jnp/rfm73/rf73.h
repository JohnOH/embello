#define RF73_MAXLEN  32

// #define RF_SELECT(n)

#ifndef RFM73
#define RFM73 1 // 0 = RFM70, 1 = RFM73
#endif

// RF73 commands
#define RF_READ_REG        	0x00  // Define read command to register
#define RF_WRITE_REG        0x20  // Define write command to register
#define RD_RX_PLOAD     		0x61  // Define RX payload register address
#define WR_TX_PLOAD     		0xA0  // Define TX payload register address
#define FLUSH_TX        		0xE1  // Define flush TX register command
#define FLUSH_RX        		0xE2  // Define flush RX register command
#define REUSE_TX_PL     		0xE3  // Define reuse TX payload register command
#define W_TX_PAYLOAD_NOACK_CMD	0xb0
#define W_ACK_PAYLOAD_CMD		0xa8
#define ACTIVATE_CMD			  0x50
#define R_RX_PL_WID_CMD			0x60
#define NOP_NOP            	0xFF  // NoOp, might be used to read status reg

// RF73 registers
#define CONFIG          0x00  // 'Config' register address
#define EN_AA           0x01  // 'Enable Auto Acknowledgment' register address
#define EN_RXADDR       0x02  // 'Enabled RX addresses' register address
#define SETUP_AW        0x03  // 'Setup address width' register address
#define SETUP_RETR      0x04  // 'Setup Auto. Retrans' register address
#define RF_CH           0x05  // 'RF channel' register address
#define RF_SETUP        0x06  // 'RF setup' register address
#define STATUS          0x07  // 'Status' register address
#define OBSERVE_TX      0x08  // 'Observe TX' register address
#define CD              0x09  // 'Carrier Detect' register address
#define RX_ADDR_P0      0x0A  // 'RX address pipe0' register address
#define RX_ADDR_P1      0x0B  // 'RX address pipe1' register address
#define RX_ADDR_P2      0x0C  // 'RX address pipe2' register address
#define RX_ADDR_P3      0x0D  // 'RX address pipe3' register address
#define RX_ADDR_P4      0x0E  // 'RX address pipe4' register address
#define RX_ADDR_P5      0x0F  // 'RX address pipe5' register address
#define TX_ADDR         0x10  // 'TX address' register address
#define RX_PW_P0        0x11  // 'RX payload width, pipe0' register address
#define RX_PW_P1        0x12  // 'RX payload width, pipe1' register address
#define RX_PW_P2        0x13  // 'RX payload width, pipe2' register address
#define RX_PW_P3        0x14  // 'RX payload width, pipe3' register address
#define RX_PW_P4        0x15  // 'RX payload width, pipe4' register address
#define RX_PW_P5        0x16  // 'RX payload width, pipe5' register address
#define FIFO_STATUS     0x17  // 'FIFO Status Register' register address
#define PAYLOAD_WIDTH   0x1f  // 'payload length of 256 bytes modes reg address

// interrupt status
#define STATUS_RX_DR 	  0x40
#define STATUS_TX_DS 	  0x20
#define STATUS_MAX_RT 	0x10
#define STATUS_TX_FULL 	0x01

// FIFO_STATUS
#define FIFO_STATUS_TX_REUSE 	0x40
#define FIFO_STATUS_TX_FULL 	0x20
#define FIFO_STATUS_TX_EMPTY 	0x10
#define FIFO_STATUS_RX_FULL 	0x02
#define FIFO_STATUS_RX_EMPTY 	0x01

const uint8_t bank0_init [] = {
    1, 0, 0x0F,
    1, 1, 0x3F,
    1, 2, 0x3F,
    1, 3, 0x03,
    1, 4, 0xFF,
    1, 5, 0x17,
#if RFM73
    1, 6, 0x00,
#else
    1, 6, 0x07,
#endif
    1, 7, 0x07,
    1, 8, 0x00,
    1, 9, 0x00,
    5, 10, 0x4A,0x4C,0x4D,0x77,0x01,
    5, 11, 0x4A,0x4C,0x4D,0x77,0x02,
    5, 16, 0x4A,0x4C,0x4D,0x77,0x01,
    1, 23, 0x00,
    1, 28, 0x3F,
    1, 29, 0x07,
    0,
};

const uint8_t bank1_init [] = {
    4, 0, 0x40,0x4B,0x01,0xE2,
    4, 1, 0xC0,0x4B,0x00,0x00,
    4, 2, 0xD0,0xFC,0x8C,0x02,
    4, 3, 0x99,0x00,0x39,0x41,
#if RFM73
    4, 4, 0xD9,0x9E,0x86,0x0B,
#else
    4, 4, 0xD9,0x96,0x82,0x1B,
#endif
    4, 5, 0x24,0x02,0x7F,0xA6,
    4, 12, 0x00,0x12,0x73,0x00,
#if RFM73
    4, 13, 0x36,0xB4,0x80,0x00,
#else
    4, 13, 0x46,0xB4,0x80,0x00,
#endif
    11, 14, 0x41,0x20,0x08,0x04,0x81,0x20,0xCF,0xF7,0xFE,0xFF,0xFF,
    0
};

template< typename SPI, int SELPIN >
class RF73 {
public:
    void init (uint8_t chan);

    int receive (void* ptr, int len);
    void send (uint8_t ack, const void* ptr, int len);

private:
    void configure (const uint8_t* p);

    void select () {
        LPC_GPIO_PORT->B[0][SELPIN] = 0; // TODO this is architecture-specific
    }

    void deselect () {
        LPC_GPIO_PORT->B[0][SELPIN] = 1; // TODO this is architecture-specific
    }

    void readBuf (uint8_t addr, uint8_t *pBuf, uint8_t length) {
#if 0
        spi.enable();
        //for (int i = 0; i < 10; ++i) __ASM("");
        spi.transfer(addr);
        //for (int i = 0; i < 10; ++i) __ASM("");
        for (uint8_t i = 0; i < length; ++i) {
            pBuf[i] = spi.transfer(0);
            //for (int i = 0; i < 10; ++i) __ASM("");
        }
        spi.disable();
#else
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_TXRDY))
            ;
        Chip_SPI_SendFirstFrame(spi.addr(), addr, 8);
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_RXRDY))
            ;
        Chip_SPI_ReceiveFrame(spi.addr());
        for (uint8_t i = 0; i < length - 1; ++i) {
            while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_TXRDY))
                ;
            Chip_SPI_SendMidFrame(spi.addr(), 0);
            while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_RXRDY))
                ;
            pBuf[i] = Chip_SPI_ReceiveFrame(spi.addr());
        }
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_TXRDY))
            ;
        Chip_SPI_SendLastFrame(spi.addr(), 0, 8);
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_RXRDY))
            ;
        pBuf[length-1] = Chip_SPI_ReceiveFrame(spi.addr());
#endif
    }

    void writeBuf (uint8_t addr, const uint8_t *pBuf, uint8_t length) {
#if 0
        spi.enable();
        //for (int i = 0; i < 10; ++i) __ASM("");
        if (addr < 32)
            addr |= RF_WRITE_REG;
        spi.transfer(addr);
        //for (int i = 0; i < 10; ++i) __ASM("");
        for (uint8_t i = 0; i < length; ++i) {
            spi.transfer(pBuf[i]);
            //for (int i = 0; i < 10; ++i) __ASM("");
        }
        spi.disable();
#else
        if (addr < 32)
            addr |= RF_WRITE_REG;
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_TXRDY))
            ;
        Chip_SPI_SendFirstFrame_RxIgnore(spi.addr(), addr, 8);
        for (uint8_t i = 0; i < length - 1; ++i) {
            while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_TXRDY))
                ;
            Chip_SPI_SendMidFrame(spi.addr(), pBuf[i]);
        }
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_TXRDY))
            ;
        Chip_SPI_SendLastFrame(spi.addr(), pBuf[length-1], 8);
        while (!(Chip_SPI_GetStatus(spi.addr()) & SPI_STAT_RXRDY))
            ;
        Chip_SPI_ReceiveFrame(spi.addr());
#endif
    }

    uint8_t readReg (uint8_t addr) {
#if 0
        uint8_t val;
        readBuf(addr, &val, 1);
        return val;
#else
        return spi.rwReg(addr, 0);
#endif
    }

    void writeReg (uint8_t addr, uint8_t val) {
#if 0
        writeBuf(addr, &val, 1);
#else
        spi.rwReg(addr, val);
#endif
    }

    void rxMode () {
        //writeReg(FLUSH_RX, 0);
        select();
        writeReg(CONFIG, readReg(CONFIG) | 1);
        deselect();
    }

    void txMode () {
        writeReg(FLUSH_TX, 0);
        select();
        writeReg(CONFIG, readReg(CONFIG) & ~1);
        deselect();
    }

    void setBank (char bank) {
        if (bank != (readReg(7) >> 7))
            writeReg(ACTIVATE_CMD, 0x53);
    }

    SPI spi;
};

template< typename SPI, int SELPIN >
void RF73<SPI,SELPIN>::init (uint8_t chan) {
    deselect();
    LPC_GPIO_PORT->DIR[0] |= 1<<SELPIN; // define select pin as output
    spi.master(3);

    setBank(0);
    if (readReg(29) == 0)
        writeReg(ACTIVATE_CMD, 0x73);

    configure(bank0_init);
    writeReg(5, chan);

    writeReg(CONFIG, 0x7F); // power up
    tick.delay(10);

    setBank(1);
    configure(bank1_init);

    setBank(0);
    rxMode();
}

template< typename SPI, int SELPIN >
void RF73<SPI,SELPIN>::configure (const uint8_t* data) {
    while (*data) {
        uint8_t len = *data++;
        uint8_t reg = *data++;
        writeBuf(reg, data, len);
        data += len;
    }
}

template< typename SPI, int SELPIN >
int RF73<SPI,SELPIN>::receive (void* ptr, int len) {
    if (readReg(STATUS) & STATUS_RX_DR) {
        do {
            uint8_t bytes = readReg(R_RX_PL_WID_CMD);
            if (bytes > 0) {
                if (bytes <= len) {
                    readBuf(RD_RX_PLOAD, (uint8_t*) ptr, bytes);
                    writeReg(FLUSH_RX, 0);
                    return bytes;
                }
                writeReg(FLUSH_RX, 0);
            }
        } while ((readReg(FIFO_STATUS) & FIFO_STATUS_RX_EMPTY) == 0);
    }
    return -1;
}

template< typename SPI, int SELPIN >
void RF73<SPI,SELPIN>::send (uint8_t ack, const void* ptr, int len) {
    const uint8_t* pbuf = (const uint8_t*) ptr;
    txMode();
    writeBuf(ack ? WR_TX_PLOAD : W_TX_PAYLOAD_NOACK_CMD, pbuf, len);
    rxMode();
}
