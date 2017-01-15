#pragma once

#include "packets.h"
#include "util.h"

template< typename DRIVER >
class BootServer {
    DRIVER driver;

    int hello (const HelloRequest* ip, HelloReply* op) {
        op->type = ip->type;
        op->bootRev = ip->bootRev;
        op->swId = driver.selectCode(ip->type, ip->hwId);
        const uint8_t* data = driver.loadFile(op->swId, &op->swSize);
        op->swCrc = Util::calculateCrc(CRC_INIT, data, op->swSize);
        return sizeof *op;
    }

    int fetch (const FetchRequest* ip, FetchReply* op) {
        uint16_t size;
        const uint8_t* data = driver.loadFile(ip->swId, &size);
        const int chunkSize = sizeof op->data;
        int pos = ip->swIndex * chunkSize;
        int len = size - pos;
        if (len < 0)
            len = 0;
        if (len > chunkSize)
            len = chunkSize;
        op->swIdXor = ip->swId ^ ip->swIndex;
        if (len > 0)
            memcpy(op->data, data + pos, (size_t) len);
        return 2 + len;
    }

public:
    int request (const void* inp, unsigned inLen, BootReply* outp) {
        if (inLen == sizeof (HelloRequest))
            return hello((const HelloRequest*) inp, &outp->h);
        if (inLen == sizeof (FetchRequest))
            return fetch((const FetchRequest*) inp, &outp->f);
        return 0;
    }
};
