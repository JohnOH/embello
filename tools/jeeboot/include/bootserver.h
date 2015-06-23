#pragma once

#include "packets.h"
#include "util.h"

template< typename D >
static int BootServerHello (const HelloRequest* ip, HelloReply* op) {
    op->type = ip->type;
    op->bootRev = ip->bootRev;
    op->swId = D::selectCode(ip->type, ip->hwId);
    const uint8_t* data = D::loadFile(op->swId, &op->swSize);
    op->swCrc = Util::calculateCrc(CRC_INIT, data, op->swSize);
    return sizeof *op;
}

template< typename D >
static int BootServerFetch (const FetchRequest* ip, FetchReply* op) {
    uint16_t size;
    const uint8_t* data = D::loadFile(ip->swId, &size);
    const int chunkSize = sizeof op->data;
    int pos = ip->swIndex * chunkSize;
    int len = size - pos;
    if (len > chunkSize)
        len = chunkSize;
    op->swIdXor = ip->swId ^ ip->swIndex;
    if (len > 0)
        memcpy(op->data, data + pos, (size_t) len);
    return 2 + len;
}

template< typename D >
static int BootServerRequest (const void* inp, unsigned inLen, BootReply* outp) {
    if (inLen == sizeof (HelloRequest))
        return BootServerHello<D>((const HelloRequest*) inp, &outp->h);
    if (inLen == sizeof (FetchRequest))
        return BootServerFetch<D>((const FetchRequest*) inp, &outp->f);
    return 0;
}
