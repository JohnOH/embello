#include "fileaccess.h"

#include "CppUTest/TestHarness.h"

static const uint8_t oneHwId [16] = {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
static const uint8_t twoHwId [16] = {2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
static const uint8_t incHwId [16] = {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};

TEST_GROUP(FileAccess)
{
    FileAccess* fileAccessPtr;

    void setup () {
        fileAccessPtr = new FileAccess ("files/");
    }

    void teardown () {
        delete fileAccessPtr;
    }
};

TEST(FileAccess, SelectUnknown) {
    uint16_t swid = fileAccessPtr->selectCode(0, 0);
    CHECK_EQUAL(65500, swid);
}

TEST(FileAccess, SelectIgnoreType) {
    uint16_t swid = fileAccessPtr->selectCode(11, 0);
    CHECK_EQUAL(65500, swid);
}

TEST(FileAccess, SelectHwIdOne) {
    uint16_t swid = fileAccessPtr->selectCode(33, oneHwId);
    CHECK_EQUAL(65501, swid);
}

TEST(FileAccess, SelectMissingType) {
    uint16_t swid = fileAccessPtr->selectCode(22, twoHwId);
    CHECK_EQUAL(22, swid);
}

TEST(FileAccess, SelectHwIdInc) {
    uint16_t swid = fileAccessPtr->selectCode(44, incHwId);
    CHECK_EQUAL(65503, swid);
}

TEST(FileAccess, Load_65500) {
    uint16_t size;
    const uint8_t* data = fileAccessPtr->loadFile(65500, &size);
    CHECK_EQUAL(122, size);
    const char* expectedData =
        "abcdefghijklmnopqrstuvwxyz\n"
        "++++++++++++++++++++++++++\n"
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ\n"
        "\1\0\2\0\3\0\4\0\5\0\6\0\7\n"
        "line above has binary data\n";
    MEMCMP_EQUAL(expectedData, data, size);
}

TEST(FileAccess, LoadMissing) {
    uint16_t size;
    const uint8_t* data = fileAccessPtr->loadFile(65501, &size);
    CHECK_EQUAL(0, size);
    CHECK_EQUAL(0, data);
}
