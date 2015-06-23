#include "fakedata.h"
#include "bootlogic.h"

#include "CppUTest/TestHarness.h"

const uint16_t MOCK_SWID = 54321;

static FakeData* fakedataPtr;
static int driverCalls;
static int callbackCalls;
static uint16_t runningCrc;
static int finalPos;

class MockDriver {
    int hello (const HelloRequest* ip, HelloReply* op) {
        ++driverCalls;
        op->type = ip->type;
        op->bootRev = ip->bootRev;
        op->swId = MOCK_SWID;
        op->swSize = fakedataPtr->size;
        op->swCrc = fakedataPtr->crc;
        return sizeof *op;
    }

    int fetch (const FetchRequest* ip, FetchReply* op) {
        ++driverCalls;
        op->swIdXor = ip->swId ^ ip->swIndex;
        int len = fakedataPtr->getData(ip->swIndex * 43); // FIXME: hack!
        memcpy(op, fakedataPtr->buf, (size_t) len);
        runningCrc = Util::calculateCrc(runningCrc, op, len);
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

static bool MockDispatch (int pos, const uint8_t* buf, int len) {
    ++callbackCalls;
    if (buf == 0 && len == 0)
        finalPos = pos;
    return true;
}

TEST_GROUP(BootLogic)
{
    BootLogic<MockDriver,MockDispatch> bootLogic;
    FakeData fakeData;

    void setup () {
        driverCalls = callbackCalls = 0;
        runningCrc = CRC_INIT;
        finalPos = 0;
        fakedataPtr = &fakeData;
        fakeData.prepare();
    }
};

TEST(BootLogic, CheckStructSizes)
{
    CHECK_EQUAL(18, sizeof (HelloRequest));
    CHECK_EQUAL(8, sizeof (HelloReply));
    CHECK_EQUAL(4, sizeof (FetchRequest));
    CHECK_EQUAL(62, sizeof (FetchReply));
    CHECK_EQUAL(64, sizeof (BootReply));
}

TEST(BootLogic, TryHello)
{
    bool ok = bootLogic.identify(99);
    CHECK_TRUE(ok);
    CHECK_EQUAL(1, driverCalls);

    HelloReply expect = { 99, 1, MOCK_SWID, fakeData.size, fakeData.crc };
    MEMCMP_EQUAL(&expect, &bootLogic.reply.h, sizeof bootLogic.reply.h);
}

TEST(BootLogic, TryHelloWithHwid)
{
    static const uint8_t myHwid [16] = {
        1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16
    };
    bool ok = bootLogic.identify(99, myHwid);
    CHECK_TRUE(ok);
    CHECK_EQUAL(1, driverCalls);

    HelloReply expect = { 99, 1, MOCK_SWID, fakeData.size, fakeData.crc };
    MEMCMP_EQUAL(&expect, &bootLogic.reply.h, sizeof bootLogic.reply.h);
}

TEST(BootLogic, FetchIndexZero)
{
    int len = bootLogic.fetchOne(MOCK_SWID, 0);
    CHECK_EQUAL(43, len);
    MEMCMP_EQUAL(fakeData.buf + 2, bootLogic.reply.f.data, (size_t) len - 2);
}

TEST(BootLogic, FetchIndexOne)
{
    int len = bootLogic.fetchOne(MOCK_SWID, 1);
    CHECK_EQUAL(43, len);
    MEMCMP_EQUAL(fakeData.buf + 2, bootLogic.reply.f.data, (size_t) len - 2);
}

TEST(BootLogic, FetchAll)
{
    bool ok = bootLogic.fetchAll(MOCK_SWID);
    CHECK_EQUAL(4, driverCalls);    // 2 full, 1 partial, 1 empty
    CHECK_EQUAL(4, callbackCalls);  // 3 with data, 1 to finish off
    CHECK_TRUE(ok);
    CHECK_EQUAL(fakeData.size, finalPos);
    CHECK_EQUAL(fakeData.crc, runningCrc);
}
