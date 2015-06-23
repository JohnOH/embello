// Raw packet bridge between an RFM69 and MQTT, as well as JeeBoot server.
// See sibling "rf69mqtt" and "rf69boot" projects for details.

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>

#include "util.h"
#include "fileaccess.h"
#include "bootserver.h"

#include <wiringPi.h>
#include <wiringPiSPI.h>

#include <mosquittopp.h>

#define DEBUG   0             // prints all incoming packets to stdout if set
#define NAME    "rf69"        // name of this client, also used in topic
#define SERVER  "127.0.0.1"   // which MQTT server to connected to

// fixed configuration settings for now
#define RF_FREQ   8686
#define RF_GROUP  42
#define RF_ID     62

class MyMqtt : public mosquittopp::mosquittopp {
public:
    MyMqtt () : mosquittopp::mosquittopp (NAME) { MyMqtt::lib_init(); }
    virtual void on_connect (int rc) { printf("connected %d\n", rc); }
    virtual void on_disconnect () { printf("disconnected\n"); }
    virtual void on_message (const struct mosquitto_message* msg);
};

MyMqtt mqtt;
char myTopic [20];

#include "spi.h"
#include "rf69.h"

RF69<SpiDev0> rf;

void MyMqtt::on_message (const struct mosquitto_message* msg) {
    uint8_t hdr = (msg->payload[0] & 0x3F) | (msg->payload[1] & 0xC0);
    rf.send(hdr, msg->payload + 2, msg->payloadlen - 2);
}

const char* filePath;

class MyFileAccess : public FileAccess {
public:
    MyFileAccess () : FileAccess (filePath) {}
};

int main () {
    if (argc < 2) {
        fprintf(stderr, "Usage: rf69boot <filepathprefix>\n");
        return 1;
    }

    filePath = argv[1];
    printf("\n[rf69boot] using: %s*\n", filePath);

    sprintf(myTopic, "raw/%s/%d-%d", NAME, RF_FREQ, RF_GROUP);
    printf("\n[rf69mqtt] %s @ %s\n", myTopic, SERVER);

    wiringPiSetup();
    if (wiringPiSPISetup (0, 4000000) < 0) {
        printf("Can't open the SPI bus: %d\n", errno);
        return 1;
    }

    rf.init(RF_ID, RF_GROUP, RF_FREQ);
    //rf.encrypt("mysecret");
    rf.txPower(15); // 0 = min .. 31 = max

    BootServer<MyFileAccess> server;

    mqtt.connect(SERVER);
    mqtt.subscribe(0, myTopic);

    struct {
        int16_t afc;
        uint8_t rssi;
        uint8_t lna;
        uint8_t buf [64];
    } rx;

    while (true) {
        int len = rf.receive(rx.buf, sizeof rx.buf);
        if (len >= 0) {
#if DEBUG
            printf("OK ");
            for (int i = 0; i < len; ++i)
                printf("%02x", rx.buf[i]);
            printf(" (%d%s%d:%d)\n",
                    rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
#endif

            if (buf[1] == 0xC0) {
                BootReply reply;
                int len2 = server.request(buf + 2, len - 2, &reply);
                if (len2 <= 0) {
                    printf("ignoring %d bytes\n", len - 2);
                } else {
                    printf("sending %d bytes\n", len2);
                    rf.send(0xC0, (const uint8_t*) &reply + 2, len2);
                }
                continue;
            }

            rx.afc = rf.afc;
            rx.rssi = rf.rssi;
            rx.lna = rf.lna;

            char topic [30];
            sprintf(topic, "%s/%d", myTopic, rx.buf[1] & 0x3F);
            mqtt.publish(0, topic, 4 + len, (const uint8_t*) &rx);
        }

        chThdYield();
    }
}
