// I2C master, reads out an attached RTC chip on I2C address 0x68.
// See http://jeelabs.org/2014/12/24/eye-squared-see/

#include "stdio.h"
#include "serial.h"

#include "lpc_types.h"
#include "romapi_8xx.h"

uint32_t i2cBuffer [24];
I2C_HANDLE_T* ih;

extern "C" void SysTick_Handler () {                                             
    // the only effect is to generate an interrupt, no work is done here         
}

void delay (int millis) {
    while (--millis >= 0)
        __WFI(); // wait for the next SysTick interrupt
}

void i2cSetup () {
    LPC_SWM->PINENABLE0 = 3<<2;             // disable SWCLK and SWDIO
    LPC_SWM->PINASSIGN7 = 0x02FFFFFF;       // SDA on P2, pin 4
    LPC_SWM->PINASSIGN8 = 0xFFFFFF03;       // SCL on P3, pin 3
    LPC_SYSCON->SYSAHBCLKCTRL |= (1<<5);    // enable I2C clock

    ih = LPC_I2CD_API->i2c_setup(LPC_I2C_BASE, i2cBuffer);
    LPC_I2CD_API->i2c_set_bitrate(ih, 12000000, 400000);
}

const uint8_t* i2cSendRecv (uint8_t addr, uint8_t reg, int len) {
    static uint8_t buf [10];

    I2C_PARAM_T param;
    I2C_RESULT_T result;

    buf[0] = (addr << 1) | 1;
    buf[1] = reg;

    /* Setup parameters for transfer */
    param.num_bytes_send  = 2;
    param.num_bytes_rec   = 8;
    param.buffer_ptr_send = param.buffer_ptr_rec = buf;
    param.stop_flag       = 1;

    LPC_I2CD_API->i2c_set_timeout(ih, 100000);
    LPC_I2CD_API->i2c_master_tx_rx_poll(ih, &param, &result);

    printf("20%02x/%02x/%02x %02x:%02x:%02x %02x\n",
        buf[7], buf[6], buf[5], buf[3], buf[2], buf[1], buf[4]);

    return buf;
}

int main () {
    // send UART output to GPIO 4, running at 115200 baud
    LPC_SWM->PINASSIGN0 = 0xFFFFFF04UL;
    serial.init(LPC_USART0, 115200);

    printf("\n[master]\n");
    SysTick_Config(12000000/1000); // 1000 Hz
    i2cSetup();

    while (true) {
        i2cSendRecv(0x68, 0, 1);
        delay(1000);
    }
}
