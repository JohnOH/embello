// Legacy mode RF69 driver.

#ifndef chThdYield
#define chThdYield() // FIXME should be renamed, ChibiOS leftover
#endif
#define RF69_SPI_BULK false

template< typename SPI >
class RF69 {
  public:
    void init (uint8_t id, uint8_t group, int freq);
    void encrypt (const char* key);
    void txPower (uint8_t level);
    uint16_t crc_ccitt_update (uint16_t crc, uint8_t data);
	void delay_3t (uint64_t cycles);

    int receive (void* ptr, int len);
    uint send (uint8_t header, const void* ptr, int len);
    void sleep ();

    int16_t afc;
    uint8_t rssi;
    uint8_t lna;
    uint8_t myId;
    uint8_t group;
    uint8_t parity;
    
    uint16_t crc;  

    uint8_t readReg (uint8_t addr) {
      return spi.rwReg(addr, 0);
    }
    void writeReg (uint8_t addr, uint8_t val) {
      spi.rwReg(addr | 0x80, val);
    }

  protected:
    enum {
      REG_FIFO          = 0x00,
      REG_OPMODE        = 0x01,
      REG_FRFMSB        = 0x07,
      REG_PALEVEL       = 0x11,
      REG_LNAVALUE      = 0x18,
      REG_AFCMSB        = 0x1F,
      REG_AFCLSB        = 0x20,
      REG_FEIMSB        = 0x21,
      REG_FEILSB        = 0x22,
      REG_RSSIVALUE     = 0x24,
      REG_IRQFLAGS1     = 0x27,
      REG_IRQFLAGS2     = 0x28,
      REG_SYNCVALUE1    = 0x2F,
      REG_SYNCVALUE2    = 0x30,
      REG_SYNCVALUE4	= 0x32,
	  REG_SYNCVALUE5    = 0x33,
      REG_NODEADDR      = 0x39,
      REG_BCASTADDR     = 0x3A,
      REG_FIFOTHRESH    = 0x3C,
      REG_PKTCONFIG2    = 0x3D,
      REG_AESKEYMSB     = 0x3E,

      MODE_SLEEP        = 0<<2,
      MODE_STANDBY      = 1<<2,
      MODE_TRANSMIT     = 3<<2,
      MODE_RECEIVE      = 4<<2,

      START_TX          = 0xC2,
      STOP_TX           = 0x42,

      RCCALSTART        = 0x80,
      IRQ1_MODEREADY    = 1<<7,
      IRQ1_RXREADY      = 1<<6,
      IRQ1_SYNADDRMATCH = 1<<0,

	  IRQ2_FIFOFULL     = 1<<7,
      IRQ2_FIFONOTEMPTY = 1<<6,
	  IRQ2_FIFOLEVEL	= 1<<5,
      IRQ2_FIFOOVERRUN  = 1<<4,
      IRQ2_PACKETSENT   = 1<<3,
      IRQ2_PAYLOADREADY = 1<<2,
    };

    void setMode (uint8_t newMode);
    void configure (const uint8_t* p);
    void setFrequency (uint32_t freq);

    SPI spi;
    volatile uint8_t mode;
};

// driver implementation

template< typename SPI >
void RF69<SPI>::setMode (uint8_t newMode) {
  mode = newMode;
  writeReg(REG_OPMODE, (readReg(REG_OPMODE) & 0xE3) | newMode);
  while ((readReg(REG_IRQFLAGS1) & IRQ1_MODEREADY) == 0)
    ;
}

template< typename SPI >
void RF69<SPI>::setFrequency (uint32_t hz) {
  // accept any frequency scale as input, including KHz and MHz
  // multiply by 10 until freq >= 100 MHz (don't specify 0 as input!)
  while (hz < 100000000)
    hz *= 10;

  // Frequency steps are in units of (32,000,000 >> 19) = 61.03515625 Hz
  // use multiples of 64 to avoid multi-precision arithmetic, i.e. 3906.25 Hz
  // due to this, the lower 6 bits of the calculated factor will always be 0
  // this is still 4 ppm, i.e. well below the radio's 32 MHz crystal accuracy
  // 868.0 MHz = 0xD90000, 868.3 MHz = 0xD91300, 915.0 MHz = 0xE4C000
  uint32_t frf = (hz << 2) / (32000000L >> 11);
  writeReg(REG_FRFMSB, frf >> 10);
  writeReg(REG_FRFMSB+1, frf >> 2);
  writeReg(REG_FRFMSB+2, frf << 6);
}

template< typename SPI >
void RF69<SPI>::configure (const uint8_t* p) {
  while (true) {
    uint8_t cmd = p[0];
    if (cmd == 0)
      break;
    writeReg(cmd, p[1]);
    p += 2;
  }
}

static const uint8_t configRegs [] = {
// POR value is better for first rf_sleep  0x01, 0x00, // OpMode = sleep
  0x02, 0x00, // DataModul = packet mode, fsk
  0x03, 0x02, // BitRateMsb, data rate = 49,261 khz
  0x04, 0x8A, // BitRateLsb, divider = 32 MHz / 650 == 49,230 khz
  0x05, 0x05, // FdevMsb = 90 KHz
  0x06, 0xC3, // FdevLsb = 90 KHz

  0x0B, 0x20, // AfcCtrl, afclowbetaon

  0x19, 0xE2, // RxBw 125 KHz, if DCC set to 0 is more sensitive
  0x1A, 0xF7, // RxBwAFC 2.6 Khz Only handling initial RSSI phase, not payload!

  0x1E, 0x00, // 

  0x26, 0x07, // disable clkout

  0x29, 0xFF, // RssiThresh ... -127.5dB

  0x2E, 0x98, // SyncConfig = sync on, sync size = 4
  0x2F, 0xAA, // SyncValue1 = 0xAA
  0x30, 0xAA, // SyncValue2 = 0xAA
  0x31, 0x2D, // SyncValue3 = 0x2D
  0x32, 0xD4, // SyncValue4 = 212, Group
  
  0x37, 0x00, // PacketConfig1 = fixed, no crc, filt off
  0x38, 0x00, // PayloadLength = 0, unlimited
  0x3C, 0x85, // at least four bytes in the FIFO :id:len=0:crc-l:crc-h:
  0x3D, 0x10, // PacketConfig2, interpkt = 1, autorxrestart off
  0x6F, 0x20, // TestDagc ...
  0
};

template< typename SPI >
void RF69<SPI>::init (uint8_t id, uint8_t groupid, int freq) {
  myId = id;

  // 10 MHz, i.e. 30 MHz / 3 (or 4 MHz if clock is still at 12 MHz)
  spi.master(1024);
/*
  do
    writeReg(REG_SYNCVALUE1, 0xAA);
  while (readReg(REG_SYNCVALUE1) != 0xAA);
  do
    writeReg(REG_SYNCVALUE1, 0x55);
  while (readReg(REG_SYNCVALUE1) != 0x55);
*/
  configure(configRegs);
  setFrequency(freq);

  group = groupid;
  writeReg(REG_SYNCVALUE4, group);
  
  }

template< typename SPI >
void RF69<SPI>::encrypt (const char* key) {
  uint8_t cfg = readReg(REG_PKTCONFIG2) & ~0x01;
  if (key) {
    for (int i = 0; i < 16; ++i) {
      writeReg(REG_AESKEYMSB + i, *key);
      if (*key != 0)
        ++key;
    }
    cfg |= 0x01;
  }
  writeReg(REG_PKTCONFIG2, cfg);
}

template< typename SPI >
void RF69<SPI>::txPower (uint8_t level) {
  writeReg(REG_PALEVEL, (readReg(REG_PALEVEL) & ~0x1F) | level);
}

template< typename SPI >
void RF69<SPI>::sleep () {
  setMode(MODE_SLEEP);
}

template< typename SPI >
int RF69<SPI>::receive (void* ptr, int len) {
	if (mode != MODE_RECEIVE) {
		setMode(MODE_SLEEP);
		writeReg(REG_IRQFLAGS2, IRQ2_FIFOOVERRUN);  	// Clear FIFO
    	setMode(MODE_RECEIVE);
    } else {
		static uint8_t lastFlag;
		uint8_t dest;
 		if ((readReg(REG_IRQFLAGS1) & IRQ1_RXREADY) != lastFlag) {
			lastFlag ^= IRQ1_RXREADY;
			if (lastFlag) { // flag just went from 0 to 1
				rssi = readReg(REG_RSSIVALUE);
				lna = (readReg(REG_LNAVALUE) >> 3) & 0x7;
#if RF69_SPI_BULK
				spi.enable();
				spi.transfer(REG_AFCMSB);
				afc = spi.transfer(0) << 8;
				afc |= spi.transfer(0);
				spi.disable();
#else
				afc = readReg(REG_AFCMSB) << 8;
				afc |= readReg(REG_AFCLSB);
#endif
			}
		}

    	if (readReg(REG_IRQFLAGS2) & IRQ2_FIFOLEVEL) {	// Min 3 bytes in FIFO?
			((uint8_t*) ptr)[0] = group;
			crc = crc_ccitt_update(~0, group);		// Group number in CRC

#if RF69_SPI_BULK
			spi.enable();
			spi.transfer(REG_FIFO);
		
			dest = spi.transfer(0);					// Target Id
			((uint8_t*) ptr)[1] = dest;
			crc = crc_ccitt_update(crc, dest);
			int count = spi.transfer(0);			// Data bytes	
			if (count <= 66) {
				((uint8_t*) ptr)[2] = count;	
				crc = crc_ccitt_update(crc, count);
				for (int i = 0; i < (count + 2); ++i) {
					v = spi.transfer(0);
        			if (i < len) {
          				((uint8_t*) ptr)[i + 3] = v;
						crc = crc_ccitt_update(crc, v);
						// might need < 0.16uS delay/byte to keep FIF0 ahead of RF
					}
				}
      		}
      	
      		spi.disable();
#else

			dest = readReg(REG_FIFO);				// Target Id
			((uint8_t*) ptr)[1] = dest;
			crc = crc_ccitt_update(crc, dest);
			int count = readReg(REG_FIFO);			// Data bytes
			if (count <= 66) {
				((uint8_t*) ptr)[2] = count;
				crc = crc_ccitt_update(crc, count);
				for (int i = 0; i < (count + 2); ++i) {
        			uint8_t v = readReg(REG_FIFO);
        			if (i < len) {
						((uint8_t*) ptr)[i + 3] = v;
						crc = crc_ccitt_update(crc, v);
						// might need < 0.16uS delay/byte to keep FIFO ahead of RF
//						printf(" %u", v);
						for (int l = 0; l < 300; ++l) {	// @ 8MHz
//						for (int l = 0; l < 2000; ++l) {	// @ 72MHz
							asm("");			
						}
					}
				}
//			putchar('\n');

      		}
//			putchar('\n');
#endif

			setMode(MODE_SLEEP);
			writeReg(REG_IRQFLAGS2, IRQ2_FIFOOVERRUN);  	// Clear FIFO
    		setMode(MODE_RECEIVE);
//			printf("%02x", count);
//			putchar('\n');
			if (!crc) {
	      		// only accept packets intended for us, or broadcasts
	      		// ... or any packet if we're the special catch-all node
      	      	
//    			uint8_t dest = *(uint8_t*) ptr + 1;
//    			if (dest == myId || dest == 0 || myId == 31) return count;
//    			if (dest == myId || myId == 31) return count;
				return count;
    		} else {
				printf("Bad CRC i%u l=%u", (dest & 0x1F), count);
				putchar('\n');
    		}
  		}
	return -1;
	}
}

template< typename SPI >
uint RF69<SPI>::send (uint8_t header, const void* ptr, int len) {
	setMode(MODE_SLEEP);
	writeReg(REG_IRQFLAGS2, IRQ2_FIFOOVERRUN);  // Clear FIFO
	crc = crc_ccitt_update(~0, group);			// Group number in CRC
#if RF69_SPI_BULK
	spi.enable();
	spi.transfer(REG_FIFO | 0x80);
	spi.transfer(myId);
	crc = crc_ccitt_update(crc, myId);
	spi.transfer(len);
	crc = crc_ccitt_update(crc, len);
	for (int i = 0; i < len; i++) {
		spi.transfer(((const uint8_t) ptr)[i]);
		crc = crc_ccitt_update(crc, ((const uint8_t*) ptr)[i]);
	}
	spi.transfer((const uint8_t) crc);
	spi.transfer((const uint8_t) crc >> 8);
	spi.disable();
#else
	writeReg(REG_FIFO, myId);
	crc = crc_ccitt_update(crc, myId);
	writeReg(REG_FIFO, len);
	crc = crc_ccitt_update(crc, len);
	for (int i = 0; i < len; i++) {
		writeReg(REG_FIFO, ((const uint8_t*) ptr)[i]);
		crc = crc_ccitt_update(crc, ((const uint8_t*) ptr)[i]);
	}
	writeReg(REG_FIFO, (const uint8_t) crc);
	writeReg(REG_FIFO, (const uint8_t) (crc >> 8));
#endif

	setMode(MODE_TRANSMIT);
	while ((readReg(REG_IRQFLAGS2) & IRQ2_PACKETSENT) == 0)
		chThdYield();

	setMode(MODE_STANDBY);
}

/*	With thanks to Jean-Claude Wippler for this CRC algorithm as used on legacy Jeenodes
	https://github.com/jcw/jeecode/blob/007e18248aed0f02e2c0d31423d9e2b074f1852b/t-radio/rf69c/radio-compat.cpp#L12-L22
	The NXP LPC810 hardware CRC module refers to this approach as CRC-CCITT
*/
template< typename SPI >
uint16_t RF69<SPI>::crc_ccitt_update (uint16_t i_crc, uint8_t i_data) {

    i_crc ^= i_data;
    for (int i = 0; i < 8; ++i)
        i_crc = (i_crc >> 1) ^ (0xA001 * (i_crc & 1));
    return i_crc;

/*
// table lookup is more concise and probably faster, but generates more code...
	const uint16_t crcTable [] = {
    0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401,
    0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400,
	};
	
//Crc16 from https://github.com/simpleavr/MSPNode
//static int16_t _crc16_update(uint16_t crc, uint8_t b) {
    crc = (crc >> 4) ^ crcTable[crc&0x0F] ^ crcTable[data&0x0F];
    return (crc >> 4) ^ crcTable[crc&0x0F] ^ crcTable[data>>4];	*/
}
/*
static void setup_isr(void)
{
	/* Enable EXTI0 interrupt. *
	nvic_enable_irq(BUTTON_DISCO_USER_NVIC);

	gpio_mode_setup(BUTTON_DISCO_USER_PORT, GPIO_MODE_INPUT, GPIO_PUPD_NONE,
			BUTTON_DISCO_USER_PIN);

	/* Configure the EXTI subsystem. *
	exti_select_source(BUTTON_DISCO_USER_EXTI, BUTTON_DISCO_USER_PORT);
	state.falling = false;
	exti_set_trigger(BUTTON_DISCO_USER_EXTI, EXTI_TRIGGER_RISING);
	exti_enable_request(BUTTON_DISCO_USER_EXTI);
}    
*/