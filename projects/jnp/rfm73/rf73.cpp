#include "RF73.h"

#if ARDUINO

#include "spi.h"

#define CE(x)       bitWrite(PORTB, 0, x)
#define CSN(x)      bitWrite(PORTD, 7, x)

#define CE_OUT()    bitSet(DDRB, 0)
#define CSN_OUT()   bitSet(DDRD, 7)

#else

#include "ch.h"
#include "hal.h"

#define GPIOA_RF73_CSN  9

#define CE(x)       (x ? palSetPad(GPIOA, 10) : palClearPad(GPIOA, 10))
#define CSN(x)      (x ? palSetPad(GPIOA, 9) : palClearPad(GPIOA, 9))
// #define CSN(x)      (x ? spiSelect(&SPID1) : spiUnselect(&SPID1))

#define CE_OUT()    palSetPadMode(GPIOA, 10, PAL_MODE_OUTPUT_PUSHPULL)
#define CSN_OUT()   palSetPadMode(GPIOA, 9, PAL_MODE_OUTPUT_PUSHPULL)

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
  1, 2, 0x3F,
  1, 4, 0xFF,
  1, 6, 0x07,
  5, 10, 0x4A,0x4C,0x4D,0x77,0x01,
  5, 11, 0x4A,0x4C,0x4D,0x77,0x02,
  5, 16, 0x4A,0x4C,0x4D,0x77,0x01,
  1, 28, 0x3F,
  1, 29, 0x07,
  0,
};

const uint8_t bank1_init [] = {
  4, 0, 0x40,0x4B,0x01,0xE2,
  4, 1, 0xC0,0x4B,0x00,0x00,
  4, 2, 0xD0,0xFC,0x8C,0x02,
  4, 3, 0x99,0x00,0x39,0x41,
  4, 4, 0xD9,0x96,0x82,0x1B,
  4, 5, 0x24,0x02,0x7F,0xA6,
  4, 12, 0x00,0x12,0x73,0x00,
  4, 13, 0x46,0xB4,0x80,0x00,
	11, 14, 0x41,0x20,0x08,0x04,0x81,0x20,0xCF,0xF7,0xFE,0xFF,0xFF,
//toggle REG4<25,26>
  // 1, 4, 0xDF,
  // 1, 4, 0xD9,
  0
};

static void readBuf (uint8_t reg, uint8_t *pBuf, uint8_t length) {
	CSN(0);
#if ARDUINO
	spiByte(reg);
	for (uint8_t i = 0; i < length; ++i)
		pBuf[i] = spiByte(0); 
#else
  spiSend(&SPID1, 1, &reg);
  spiReceive(&SPID1, length, pBuf);
#endif                            
	CSN(1);               
}                                                           

static void writeBuf (uint8_t reg, const uint8_t *pBuf, uint8_t length) {
	CSN(0);
  if (reg < 32)
    reg |= RF_WRITE_REG;
#if ARDUINO
	spiByte(reg);
	for (uint8_t i = 0; i < length; ++i)
    spiByte(pBuf[i]);
#else
  spiSend(&SPID1, 1, &reg);
  spiSend(&SPID1, length, pBuf);
#endif
	CSN(1);
}

static uint8_t readReg (uint8_t reg) {
  uint8_t value;
  readBuf(reg, &value, 1);
	return value;
}                                                           

static void writeReg (uint8_t reg, uint8_t value) {
	writeBuf(reg, &value, 1);
}                                                         

static void rxMode () {
	CE(0);
	writeReg(CONFIG, readReg(CONFIG) | 1);
	CE(1);
}

static void txMode () {
	CE(0);
	writeReg(CONFIG, readReg(CONFIG) & ~1); 	
	CE(1);
}

static void setBank (char bank) {
	if (bank != (readReg(7) >> 7))
		writeReg(ACTIVATE_CMD, 0x53);
}

void rf73_init (uint8_t chan) {
#if ARDUINO
  spiInit();
#else
  palSetPadMode(GPIOA, 5, PAL_MODE_STM32_ALTERNATE_PUSHPULL); // SCK
  palSetPadMode(GPIOA, 6, PAL_MODE_STM32_ALTERNATE_PUSHPULL); // MISO
  palSetPadMode(GPIOA, 7, PAL_MODE_STM32_ALTERNATE_PUSHPULL); // MOSI
  static const SPIConfig hs_spicfg = {
  	NULL,
  	GPIOA,
  	GPIOA_RF73_CSN,
  	SPI_CR1_BR_1 // fPCLK/8, 4.5 MHz, RM0008 p.713
  };
	spiStart(&SPID1, &hs_spicfg);
#endif
  
	CE(1);
	CSN(1);
	CE_OUT();
	CSN_OUT();

	setBank(0);
	if (readReg(29) == 0)
		writeReg(ACTIVATE_CMD, 0x73);
	
  rf73_configure(bank0_init);  
	writeReg(5, chan);
	
	setBank(1);
  rf73_configure(bank1_init);

	setBank(0);
	rxMode();
}

void rf73_configure (const uint8_t* data) {
  while (*data) {
    uint8_t len = *data++;
    uint8_t reg = *data++;
    writeBuf(reg, data, len);
    data += len;
  }
}

bool rf73_send (uint8_t ack, const void* buf, uint8_t len) {
  const uint8_t* pbuf = (const uint8_t*) buf;
  // writeReg(FLUSH_TX, 0);
  // writeReg(STATUS, readReg(STATUS));
  // if (readReg(FIFO_STATUS) & FIFO_STATUS_TX_FULL)
  //   return false;
	txMode();
	writeBuf(ack ? WR_TX_PLOAD : W_TX_PAYLOAD_NOACK_CMD, pbuf, len);
	rxMode();
  return true;
}

char rf73_receive (void* buf) {
  uint8_t* pbuf = (uint8_t*) buf;
  char result = -1;
	while ((readReg(FIFO_STATUS) & FIFO_STATUS_RX_EMPTY) == 0) {
		uint8_t len = readReg(R_RX_PL_WID_CMD);
		if (len <= RF73_MAXLEN) {
			readBuf(RD_RX_PLOAD, pbuf, len);
      result = len;
		}
#if ARDUINO
    delay(1);
#endif
  }
  return result;
}
