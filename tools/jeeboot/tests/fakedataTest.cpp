#include "fakedata.h"

#include "CppUTest/TestHarness.h"

TEST_GROUP(FakeData)
{
    FakeData fakeData;

    void setup () {
        fakeData.prepare();
    }
};

TEST(FakeData, GetFirstBlock)
{
    int len = fakeData.getData(0);
    CHECK_EQUAL(43, len);
}

TEST(FakeData, GetLastBlock)
{
    int size = fakeData.getData(0);
    int pos = (fakeData.size / size) * size;
    int len = fakeData.getData(pos);
    CHECK_EQUAL(fakeData.size - pos, len);
}

TEST(FakeData, CrcCalculation)
{
    CHECK_EQUAL(CRC_INIT, Util::calculateCrc(CRC_INIT, "", 0));
    CHECK_EQUAL(0x5749, Util::calculateCrc(CRC_INIT, "abc", 3));
}

TEST(FakeData, CheckTotalCrc)
{
    uint16_t csum = CRC_INIT;
    int pos = 0, len;
    do {
        len = fakeData.getData(pos);
        csum = Util::calculateCrc(csum, fakeData.buf, len);
        pos += len;
    } while (len > 0);
    CHECK_EQUAL(fakeData.crc, csum);
}
