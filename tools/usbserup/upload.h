#include <stdint.h>

enum {
    ACK = 0x79,
    NAK = 0x1F,
};

enum {
    GET_CMD = 0x00,
    GETVER_CMD = 0x01,
    GETID_CMD = 0x02,
    READ_CMD = 0x11,
    GO_CMD = 0x21,
    WRITE_CMD = 0x31,
    ERASE_CMD = 0x43,
};

static uint8_t getByte (void);
static void putByte (uint8_t b);
static void eraseFlash (void);
static void writeFlash (uint32_t addr, const uint8_t* ptr, int len);

static uint32_t getAddrCheck (void) {
    uint32_t addr = getByte() << 24;
    addr |= getByte() << 16;
    addr |= getByte() << 8;
    addr |= getByte();
    getByte(); // csum
    putByte(ACK);
    return addr;
}

static int initialSync (void) {
    int i;
    for (i = 0; i < 10; ++i) {
        uint8_t b = getByte();
        if (b == 0x7F) {
            putByte(ACK);
            return 1;
        }
    }
    putByte(NAK);
    return 0;
}

static int uploadHandler (void) {
    uint8_t cmd = getByte();
    if ((getByte() ^ cmd) != 0xFF)
        putByte(NAK);
    else
        switch (cmd) {
            case GET_CMD:
                putByte(ACK);
                putByte(7);
                putByte(0x22); // boot loader version 2.2
                putByte(GET_CMD);
                putByte(GETVER_CMD);
                putByte(GETID_CMD);
                putByte(READ_CMD);
                putByte(GO_CMD);
                putByte(WRITE_CMD);
                putByte(ERASE_CMD);
                putByte(ACK);
                break;
            case GETVER_CMD:
                putByte(ACK);
                putByte(0x22); // boot loader version 2.2
                putByte(0x00);
                putByte(0x00);
                putByte(ACK);
                break;
            case GETID_CMD:
                putByte(ACK);
                putByte(1);
                putByte(0x04);
                putByte(0x10);
                putByte(ACK);
                break;
            case ERASE_CMD: {
                putByte(ACK);
                int n = getByte();
                if (n == 0xFF) {
                    getByte();
                    eraseFlash();
                } else {
                    uint8_t buffer [256];
                    for (int i = 0; i <= n; ++i)
                        buffer[i] = getByte();
                    (void) buffer; // values ignored for now
                    getByte();
                    eraseFlash(); // still treat as full erase for now
                }
                putByte(ACK);
                break;
            }
            case WRITE_CMD: {
                putByte(ACK);
                uint32_t addr = getAddrCheck();
                int n = getByte() + 1;
                uint8_t buffer [256];
                for (int i = 0; i < n; ++i)
                    buffer[i] = getByte();
                getByte(); // csum
                writeFlash(addr, buffer, n);
                putByte(ACK);
                if (addr == 0xE000ED0C)
                    return 0;
                break;
            }
            case READ_CMD: {
                putByte(ACK);
                const uint8_t* addr = (const uint8_t*) getAddrCheck();
                int n = getByte() + 1;
                getByte(); // csum
                putByte(ACK);
                for (int i = 0; i < n; ++i)
                    putByte(addr[i]);
                break;
            }
            case GO_CMD:
                putByte(ACK);
                getAddrCheck();
                return 0; // jump, but ignore actual address
            default:
                putByte(NAK);
        }
    return 1;
}
