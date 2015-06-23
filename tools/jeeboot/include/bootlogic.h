#pragma once

#include "packets.h"

#include <string.h>

typedef bool (*BootLogicDispatch)(int, const uint8_t*, int);

template< typename DRIVER, BootLogicDispatch DISPATCH >
class BootLogic {
    DRIVER driver;

public:
    BootReply reply;

    bool identify (uint16_t type, const uint8_t* hwid =0) {
        HelloRequest req;
        req.type = type & 0xFFF;
        req.bootRev = BOOT_REVISION;

        memset(req.hwId, 0, sizeof req.hwId);
        if (hwid != 0)
            memcpy(req.hwId, hwid, sizeof req.hwId);

        int n = driver.request(&req, sizeof req, &reply);
        return n == sizeof reply.h;
    }

    int fetchOne (uint16_t swid, uint16_t index) {
        FetchRequest req;
        req.swId = swid;
        req.swIndex = index;

        return driver.request(&req, sizeof req, &reply) - 2;
    }

    bool fetchAll (uint16_t swid) {
        int pos = 0;
        uint16_t index = 0;
        for (;;) {
            int len = fetchOne(swid, index);
            if (len <= 0)
                break;
            bool ok = DISPATCH(pos, reply.f.data, len);
            if (!ok)
                return false;
            pos += len;
            ++index;
        }
        return DISPATCH(pos, 0, 0);
    }
};
