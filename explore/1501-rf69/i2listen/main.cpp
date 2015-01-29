// Report received data as I2C slave.
// See http://jeelabs.org/2015/01/28/lpc810-meets-rfm69-part-3/

#include "LPC8xx.h"
#include "lpc_types.h"
#include "romapi_8xx.h"

#define chThdYield() // FIXME still used in rf69.h
#include "spi.h"
#include "rf69.h"

#include <string.h>

RF69<SpiDevice> rf;
uint8_t rxBuf[66];

typedef struct {
    uint8_t seq, len, rssi, lna;
    uint16_t afc;
    uint8_t buf[66];
} Payload;

Payload out;

uint32_t i2cBuffer [24];
I2C_HANDLE_T* ih;
uint8_t regNum;

void i2cSetup () {
    for (int i = 0; i < 3000000; ++i) __ASM("");

    LPC_SWM->PINENABLE0 |= 1<<6;        // disable RESET, pin 1
    LPC_SWM->PINASSIGN7 = 0x05FFFFFF;   // SDA on P5
    LPC_SWM->PINASSIGN8 = 0xFFFFFF04;   // SCL on P4
    LPC_SYSCON->SYSAHBCLKCTRL |= 1<<5;  // enable I2C clock

    ih = LPC_I2CD_API->i2c_setup(LPC_I2C_BASE, i2cBuffer);
    LPC_I2CD_API->i2c_set_slave_addr(ih, 0x70<<1, 0);

    NVIC_EnableIRQ(I2C_IRQn);
}

extern "C" void I2C0_IRQHandler () {
    LPC_I2CD_API->i2c_isr_handler(ih);
}

I2C_PARAM_T i2cParam;
I2C_RESULT_T i2cResult;
uint8_t i2cRecvBuf [2];

void i2cSetupRecv (); // forward
void i2cSetupSend (); // forward

void i2cRecvDone (uint32_t err, uint32_t) {
    if (err == 0)
        regNum = i2cRecvBuf[1];
    i2cSetupRecv();
    i2cSetupSend();
}

void i2cSendDone (uint32_t, uint32_t) {
    i2cSetupRecv();
}

void i2cSetupRecv () {
    i2cParam.func_pt = i2cRecvDone;
    i2cParam.num_bytes_send = 0;
    i2cParam.num_bytes_rec = 2;
    i2cParam.buffer_ptr_rec = i2cRecvBuf;
    LPC_I2CD_API->i2c_slave_receive_intr(ih, &i2cParam, &i2cResult);
}

void i2cSetupSend () {
    i2cParam.func_pt = i2cSendDone;
    i2cParam.num_bytes_rec = 0;
    if (regNum == 0) {
        i2cParam.num_bytes_send = 1;
        i2cParam.buffer_ptr_send = &out.len;
    } else {
        i2cParam.num_bytes_send = out.len + 6;
        i2cParam.buffer_ptr_send = (uint8_t*) &out;
    }
    LPC_I2CD_API->i2c_slave_transmit_intr(ih, &i2cParam, &i2cResult);
}

int main () {
    i2cSetup();
    i2cSetupRecv();

    LPC_SWM->PINENABLE0 |= 3<<2;        // disable SWCLK/SWDIO
    // lpc810 coin: sck=0, ssel=3, miso=2, mosi=1
    LPC_SWM->PINASSIGN3 = 0x00FFFFFF;   // sck  -    -    -
    LPC_SWM->PINASSIGN4 = 0xFF030201;   // -    nss  miso mosi

    rf.init(1, 42, 8683);
    while (true) {
        int len = rf.receive(rxBuf, sizeof rxBuf);
        if (len >= 0) {
            ++out.seq;
            out.len = len;
            out.rssi = rf.rssi;
            out.lna = rf.lna;
            out.afc = rf.afc;
            memcpy(out.buf, rxBuf, sizeof out.buf);
        }
    }
}
