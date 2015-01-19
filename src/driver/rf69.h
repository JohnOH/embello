// new driver code, exposes a minimal public API

namespace RF69 {
    void init (uint32_t khz, uint8_t group, uint8_t id);
    void encrypt (const char* key);
    void txPower (uint8_t level);
    void sleep ();
    
    int receive (void* ptr, int len);
    void send (uint8_t header, const void* ptr, int len);
    
// low-level access

	enum {
		REG_FIFO          = 0x00,
		REG_OPMODE        = 0x01,
		REG_FRFMSB        = 0x07,
		REG_PALEVEL       = 0x11,
		REG_AFCMSB	      = 0x1F,
		REG_AFCLSB	      = 0x20,
		REG_FEIMSB        = 0x21,
		REG_FEILSB        = 0x22,
		REG_RSSIVALUE     = 0x24,
		REG_IRQFLAGS1     = 0x27,
		REG_IRQFLAGS2     = 0x28,
		REG_SYNCVALUE1    = 0x2F,
		REG_SYNCVALUE2    = 0x30,
		REG_NODEADDR      = 0x39,
		REG_BCASTADDR     = 0x3A,
		REG_PKTCONFIG2    = 0x3D,
		REG_AESKEYMSB     = 0x3E,
		REG_TEMP1		  = 0x4E,
		REG_TEMP2		  = 0x4F,
		
		MODE_SLEEP        = 0<<2,
		MODE_STANDBY	  = 1<<2,
		MODE_TRANSMIT     = 3<<2,
		MODE_RECEIVE      = 4<<2,

		IRQ1_MODEREADY    = 1<<7,
		IRQ1_RXREADY      = 1<<6,

		IRQ2_FIFOFULL	  = 1<<7,
		IRQ2_FIFONOTEMPTY = 1<<6,
		IRQ2_FIFOOVERRUN  = 1<<4,
		IRQ2_PACKETSENT   = 1<<3,
		IRQ2_PAYLOADREADY = 1<<2,
	};

	uint8_t readReg (uint8_t addr);
	void writeReg (uint8_t addr, uint8_t value);
	void setMode (uint8_t newMode);
	void configure (const uint8_t* p);

	extern uint8_t rssi, mode, myId, gParity;
    extern int16_t afc;
};
