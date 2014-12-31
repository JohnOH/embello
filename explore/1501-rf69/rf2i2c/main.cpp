// RF69 to I2C bridge, to buffer and report all incoming radio packets.
// See http://jeelabs.org/2014/12/31/lpc810-meets-rfm69/

#define chThdYield() // FIXME still used in radio.h

#include "spi.h"
#include "radio.h"
#include "lpc_types.h"
#include "romapi_8xx.h"

RF69<SpiDevice> rf;

class RequestHandler {
    uint32_t i2cBuf [24];
    I2C_HANDLE_T* ih;
public:
    RequestHandler (uint8_t address) {}
    bool wantsData () const { return false; }
    void replyWith (const void* ptr, int len) {}
};

RequestHandler rh (0x70);

uint32_t i2cBuffer [24];
I2C_HANDLE_T* ih;

void i2cSetupXfer(); // forward

void i2cSetup () {
    LPC_SWM->PINASSIGN7 = 0x02FFFFFF;       // SDA on P2
    LPC_SWM->PINASSIGN8 = 0xFFFFFF03;       // SCL on P3
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<5);    // enable I2C clock

    ih = LPC_I2CD_API->i2c_setup(LPC_I2C_BASE, i2cBuffer);
    LPC_I2CD_API->i2c_set_slave_addr(ih, 0x68<<1, 0);

    NVIC_EnableIRQ(I2C_IRQn);
}

extern "C" void I2C0_IRQHandler () {
    LPC_I2CD_API->i2c_isr_handler(ih);
}

void i2cDone (uint32_t, uint32_t) {
    i2cSetupXfer(); // restart the next transfer
}

void i2cSetupXfer() {
    static uint8_t buf [] = { 0, 1, 2, 3, 4, 5, 6 };
    static uint8_t seq;

    static I2C_PARAM_T param;
    static I2C_RESULT_T result;

    buf[0] = ++seq;
    buf[1] = 1; // gets overwritten by received register index

    /* Setup parameters for transfer */
    param.func_pt           = i2cDone;
    param.num_bytes_send    = 8;
    param.num_bytes_rec     = 2;
    param.buffer_ptr_send   = param.buffer_ptr_rec = buf;

    LPC_I2CD_API->i2c_slave_receive_intr(ih, &param, &result);
    LPC_I2CD_API->i2c_slave_transmit_intr(ih, &param, &result);
}

class PacketBuffer {
    uint8_t buf [500];
    uint16_t rpos, wpos;
public:
    PacketBuffer () : rpos (0), wpos (0) {}
    void add (const void* ptr, int len);
    uint8_t peek () const { return rpos != wpos ? buf[rpos] : 0; }
    const void* next () const { return buf + rpos + 1; }
    void shift () { rpos += peek() + 1; }
};

void PacketBuffer::add (const void* ptr, int len) {
}

PacketBuffer pb;

int main () {
    LPC_SWM->PINENABLE0 |= (3<<2) | (1<<6); // disable SWCLK/SWDIO and RESET

    // NSS=2, SCK=3, MISO=5, MOSI=1
    LPC_SWM->PINASSIGN3 = 0x03FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF020501;   // -    nss  miso mosi

    rf.init(1, 42, 8683);
    rf.encrypt("mysecret");

    i2cSetup();
    i2cSetupXfer();

    static struct {
        uint8_t when, rssi;
        uint16_t afc;
        uint8_t buf [66];
    } entry;

    while (true) {
        int n = rf.receive(entry.buf, sizeof entry.buf);
        if (n > 0) {
            entry.when = 0; // not used yet
            entry.rssi = rf.rssi;
            entry.afc = rf.afc;
            pb.add(&entry, 4 + n);
        }
        if (rh.wantsData()) {
            int avail = pb.peek();
            if (avail == 0)
                rh.replyWith(0, 0);
            else {
                rh.replyWith(pb.next(), avail);
                pb.shift();
            }
        }
    }
}
