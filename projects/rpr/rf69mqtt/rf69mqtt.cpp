// Raw packet bridge between an SPI-connected RFM69 and MQTT.
//
// received RF packets are published as:
//    topic = "raw/rf69/<freq>-<group>/<dstid>"
//    payload = as received, but with some extra info prefixed:
//      bytes 0..1 = afc value
//      byte 2 = rssi value
//      byte 3 = lna value
//      byte 4 = dstid (bits 0..5) + parity (bits 6..7)
//      byte 5 = srcid (bits 0..5) + hdr-bits (bits 6..7)
//      byte 6..up = actual packet payload
// each received packet is published as at least 6 raw bytes
//
// messages sent to topic = "raw/rf69/<freq>-<group>" are sent as:
//    byte 0 = dstid (bits 0..5)
//    byte 1 = hdr-bits (bits 6..7)
//    byte 2..up = actual packet payload
// a message must contain at least the above byte 0 and 1

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>

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

int main () {
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
