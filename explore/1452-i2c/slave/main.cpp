// I2C slave, responds as fake RTC device at 0x68 with some dummy data.
// See http://jeelabs.org/2014/12/24/eye-squared-see/

#include "LPC8xx.h"
#include "lpc_types.h"
#include "romapi_8xx.h"

uint32_t i2cBuffer [24];
I2C_HANDLE_T* ih;

void i2cSetupXfer(); // forward

void i2cSetup () {
    LPC_SWM->PINASSIGN7 = 0x02FFFFFF;       // SDA on P2, pin 4
    LPC_SWM->PINASSIGN8 = 0xFFFFFF03;       // SCL on P3, pin 3
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

int main () {
    i2cSetup();
    i2cSetupXfer();

    while (true)
        __WFI();
}
