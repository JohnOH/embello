#pragma once

#ifndef BOOT_DATA_MAX
#define BOOT_DATA_MAX 60
#endif

const int BOOT_REVISION = 1;

struct HelloRequest {
    uint16_t type :12;
    uint16_t bootRev :4;
    uint8_t hwId [16];
};

struct HelloReply {
    uint16_t type :12;
    uint16_t bootRev :4;
    uint16_t swId;
    uint16_t swSize;
    uint16_t swCrc;
};

struct FetchRequest {
    uint16_t swId;
    uint16_t swIndex;
};

struct FetchReply {
    uint16_t swIdXor;
    uint8_t data [BOOT_DATA_MAX];
};

struct BootReply {
    uint8_t dest :6;
    uint8_t parity :2;
    uint8_t orig :6;
    uint8_t flags :2;
    union {
        HelloReply h;
        FetchReply f;
    };
};
