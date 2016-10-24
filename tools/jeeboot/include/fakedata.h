#pragma once

#include "util.h"
#include <string.h>

class FakeData {
public:
    uint8_t* bytes;
    uint16_t size;
    uint16_t crc;
    uint8_t buf [66];

    FakeData () : bytes (0) {}
    ~FakeData () { free(bytes); }

    void prepare (uint16_t payloadSize =100) {
        bytes = (uint8_t*) realloc(bytes, payloadSize);

        srand((unsigned) time(0));
        for (int i = 0; i < size; ++i)
            bytes[i] = (uint8_t) rand();

        size = payloadSize;
        crc = Util::calculateCrc(CRC_INIT, bytes, size);
    }

    int getData (int pos) {
        const int chunkSize = 43;
        if (pos >= size || pos % chunkSize != 0)
            return 0;

        int len = size - (pos / chunkSize) * chunkSize;
        if (len > chunkSize)
            len = chunkSize;

        memcpy(buf, bytes + pos, (unsigned) len);
        return len;
    }
};
