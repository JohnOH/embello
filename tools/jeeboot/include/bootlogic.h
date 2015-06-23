#pragma once

#include "packets.h"

#include <string.h>

typedef int (*BootLogicRequest)(const void*, unsigned, BootReply*);
typedef bool (*BootLogicDispatch)(int, const uint8_t*, int);

template< BootLogicRequest R, BootLogicDispatch D >
class BootLogic {
public:
    BootReply reply;

    bool identify (uint16_t type, const uint8_t* hwid =0) {
        HelloRequest req;
        req.type = type & 0xFFF;
        req.bootRev = BOOT_REVISION;

        memset(req.hwId, 0, sizeof req.hwId);
        if (hwid != 0)
            memcpy(req.hwId, hwid, sizeof req.hwId);

        int n = R(&req, sizeof req, &reply);
        return n == sizeof reply.h;
    }

    int fetchOne (uint16_t swid, uint16_t index) {
        FetchRequest req;
        req.swId = swid;
        req.swIndex = index;

        return R(&req, sizeof req, &reply) - 2;
    }

    bool fetchAll (uint16_t swid) {
        int pos = 0;
        uint16_t index = 0;
        for (;;) {
            int len = fetchOne(swid, index);
            if (len <= 0)
                break;
            bool ok = D(pos, reply.f.data, len);
            if (!ok)
                return false;
            pos += len;
            ++index;
        }
        return D(pos, 0, 0);
    }
};
