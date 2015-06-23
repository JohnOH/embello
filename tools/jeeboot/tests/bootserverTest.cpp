#include "fakedata.h"
#include "bootserver.h"
#include "bootlogic.h"

#include "CppUTest/TestHarness.h"

const uint16_t MOCK_SWID = 12345;

static FakeData* fakedataPtr;
static uint16_t runningCrc;
static int finalPos;

class MockDriver {
public:
    uint16_t selectCode (uint16_t /*type*/, const uint8_t* /*hwid*/) {
        return MOCK_SWID;
    }

    const uint8_t* loadFile (uint16_t /*swid*/, uint16_t* sizep) {
        *sizep = fakedataPtr->size;
        return fakedataPtr->bytes;
    }
};

TEST_GROUP(BootServer)
{
    BootServer<MockDriver> bootServer;
    FakeData fakeData;

    void setup () {
        fakedataPtr = &fakeData;
        fakeData.prepare();
    }
};

TEST(BootServer, Hello)
{
    static HelloRequest req; // initialised to zeros
    BootReply reply;
    int len = bootServer.request(&req, sizeof req, &reply);
    CHECK_EQUAL(sizeof (HelloReply), len);
    CHECK_EQUAL(req.type, reply.h.type);
    CHECK_EQUAL(req.bootRev, reply.h.bootRev);
    CHECK_EQUAL(MOCK_SWID, reply.h.swId);
    CHECK_EQUAL(fakeData.size, reply.h.swSize);
    CHECK_EQUAL(fakeData.crc, reply.h.swCrc);
}

TEST(BootServer, FetchIndexZero)
{
    FetchRequest req = { MOCK_SWID, 0 };
    BootReply reply;
    int len = bootServer.request(&req, sizeof req, &reply);
    CHECK(len > 2);
    CHECK_EQUAL(MOCK_SWID ^ 0, reply.f.swIdXor);
    MEMCMP_EQUAL(fakeData.bytes, reply.f.data, (size_t) len - 2);
}

static bool MockDispatch (int pos, const uint8_t* buf, int len) {
    runningCrc = Util::calculateCrc(runningCrc, buf, len);
    if (buf == 0 && len == 0)
        finalPos = pos;
    return true;
}

TEST_GROUP(BootEndToEnd)
{
    typedef BootServer<MockDriver> FakeServer;
    BootLogic<FakeServer,MockDispatch> bootLogic;
    FakeData fakeData;

    void setup () {
        finalPos = 0;
        runningCrc = CRC_INIT;
        fakedataPtr = &fakeData;
        fakeData.prepare();
    }
};

TEST(BootEndToEnd, Hello)
{
    bool ok = bootLogic.identify(99, 0);
    CHECK_TRUE(ok);
    CHECK_EQUAL(99, bootLogic.reply.h.type);
    CHECK_EQUAL(BOOT_REVISION, bootLogic.reply.h.bootRev);
    CHECK_EQUAL(MOCK_SWID, bootLogic.reply.h.swId);
    CHECK_EQUAL(fakeData.size, bootLogic.reply.h.swSize);
    CHECK_EQUAL(fakeData.crc, bootLogic.reply.h.swCrc);
}

TEST(BootEndToEnd, FetchAll)
{
    bool ok = bootLogic.fetchAll(MOCK_SWID);
    CHECK_TRUE(ok);
    CHECK_EQUAL(fakeData.size, finalPos);
    CHECK_EQUAL(fakeData.crc, runningCrc);
}
