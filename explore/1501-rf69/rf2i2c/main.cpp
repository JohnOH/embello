// RF69 to I2C bridge, to buffer and report all incoming radio packets.
// See http://jeelabs.org/2014/12/31/lpc810-meets-rfm69/

#include "spi.h"
#include "rf69.h"
#include "lpc_types.h"
#include "romapi_8xx.h"
#include "string.h"

RF69<SpiDevice> rf;

class RequestHandler {
    uint32_t i2cBuf [24];
    I2C_HANDLE_T* ih;
public:
    RequestHandler (uint8_t address);
    bool wantsData () const { return false; }
    void replyWith (const void* ptr, int len) {}
};

RequestHandler rh (0x70);

I2C_HANDLE_T* ih;

RequestHandler::RequestHandler (uint8_t address) {
    LPC_SWM->PINASSIGN7 = 0x02FFFFFF;       // SDA on P2
    LPC_SWM->PINASSIGN8 = 0xFFFFFF03;       // SCL on P3
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<5);    // enable I2C clock

    ih = LPC_I2CD_API->i2c_setup(LPC_I2C_BASE, i2cBuf);
    LPC_I2CD_API->i2c_set_slave_addr(ih, address<<1, 0);
}

void i2cSetupXfer() {
    static uint8_t buf [] = { 0, 1, 2, 3, 4, 5, 6 };
    static uint8_t seq;

    I2C_PARAM_T param;
    I2C_RESULT_T result;

    buf[0] = ++seq;
    buf[1] = 1; // gets overwritten by received register index

    /* Setup parameters for transfer */
    param.num_bytes_send    = 8;
    param.num_bytes_rec     = 2;
    param.buffer_ptr_send   = param.buffer_ptr_rec = buf;

    LPC_I2CD_API->i2c_slave_receive_poll(ih, &param, &result);
    LPC_I2CD_API->i2c_slave_transmit_poll(ih, &param, &result);
}

class PacketBuffer {
    static const int limit = 500;
    uint8_t buf [limit+4+66]; // slack for last packet
    uint16_t rpos, wpos;
public:
    PacketBuffer () : rpos (0), wpos (0) {}
    void append (const void* ptr, int len);
    int peek () const { return rpos != wpos ? buf[rpos] : -1; }
    const void* next () const { return buf + rpos + 1; }
    void remove () { rpos += peek() + 1; if (rpos >= limit) rpos = 0; }
};

// Append len bytes to the buffer as a "packet", ignore them if they won't fit.
void PacketBuffer::append (const void* ptr, int len) {
    const int limitPos = sizeof buf - 75; // 66 and a few bytes extra

    // The rpos index is where packets are read from, wpos is where packets are
    // written to - the buffer is empty when rpos == wpos. But unlike a normal
    // byte-oriented circular buffer, this code will keep each packet in
    // contiguous memory. So the end of the buffer is not fixed - the last
    // packet in the buffer can go past the limit, since we know the maximum
    // size of packets and we've reserved some extra buffer space at the end.
    // When there is no room, packets are dropped without further notice.

    if (wpos + len + 1 < rpos || (wpos >= rpos && wpos < limit)) {
        buf[wpos++] = len;
        memcpy(buf + wpos, ptr, len);
        wpos += len;
        if (wpos >= limit)
            wpos = 0;
    }
}

PacketBuffer pb;

int main () {
    LPC_SWM->PINENABLE0 |= (3<<2) | (1<<6); // disable SWCLK/SWDIO and RESET

    // NSS=2, SCK=3, MISO=5, MOSI=1
    LPC_SWM->PINASSIGN3 = 0x03FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF020501;   // -    nss  miso mosi

    rf.init(1, 42, 8683);
    rf.encrypt("mysecret");

    i2cSetupXfer();

    struct {
        uint8_t when, rssi;
        uint16_t afc;
        uint8_t buf [66];
    } entry;

    while (true) {
        if (rh.wantsData())
            rh.replyWith("abc", 3);
    }

    while (true) {
        int n = rf.receive(entry.buf, sizeof entry.buf);
        if (n > 0) {
            entry.when = 0; // not used yet
            entry.rssi = rf.rssi;
            entry.afc = rf.afc;
            pb.append(&entry, 4 + n);
        }
        if (rh.wantsData()) {
            rh.replyWith(pb.next(), pb.peek());
            pb.remove();
        }
    }
}
