// I2C master, reads out an attached RTC chip on I2C address 0x68.

#include "embello.h"

#include "lpc_types.h"
#include "romapi_8xx.h"

uint32_t i2cBuffer [24];
I2C_HANDLE_T* ih;

void i2cSetup () {
  LPC_SWM->PINASSIGN[9] = 0xFF0D0EFF;     // SCL1 on 13p3, SDA1 on 14p20
  LPC_SYSCON->SYSAHBCLKCTRL |= (1<<21);   // enable I2C1 clock

  ih = LPC_I2CD_API->i2c_setup(LPC_I2C1_BASE, i2cBuffer);
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
  param.num_bytes_rec   = len;
  param.buffer_ptr_send = param.buffer_ptr_rec = buf;
  param.stop_flag       = 1;

  LPC_I2CD_API->i2c_set_timeout(ih, 100000);
  LPC_I2CD_API->i2c_master_tx_rx_poll(ih, &param, &result);

  printf("20%02x/%02x/%02x %02x:%02x:%02x %02x\n",
      buf[7], buf[6], buf[5], buf[3], buf[2], buf[1], buf[4]);

  return buf;
}

int main () {
  serial.init(115200);
  tick.init(1000);

  printf("\n[i2c-rtc]\n");
  i2cSetup();

  while (true) {
    i2cSendRecv(0x68, 0, 8);
    tick.delay(500);
  }
}
