#pragma once

#include <stdint.h>

const uint16_t CRC_INIT = 0xFFFF;

class Util {
public:
    static uint16_t calculateCrc(uint16_t csum, const void* ptr, int len) {
        static const uint16_t table[] = {
            0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401,
            0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400,
        };

        for (int i = 0; i < len; ++i) {
            uint8_t data = ((const uint8_t*) ptr)[i];
            csum = (uint16_t) ((csum>>4) ^ table[csum&0xF] ^ table[data&0xF]);
            csum = (uint16_t) ((csum>>4) ^ table[csum&0xF] ^ table[data>>4]);
        }
        return csum;
    }
};
