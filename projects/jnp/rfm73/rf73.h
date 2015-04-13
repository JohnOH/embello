#include <stdint.h>

#define RF73_MAXLEN  32

void rf73_init (uint8_t chan);
void rf73_configure (const uint8_t* data);
bool rf73_send (uint8_t ack, const void* buf, uint8_t len);
char rf73_receive (void* buf);
