// Raw packet bridge between an RFM70/73 and MQTT, as well as JeeBoot server.
// See sibling "rf69bridge" projects for details.

// And example of incoming messages, as published to MQTT:
//  $ mosquitto_sub -v -t '#'
//  raw/rf69/8686-42/24 "12009a038018100102030405060708090a0b0c0d0e0f"
//  raw/rf69/8686-42/24 "080099038018110102030405060708090a0b0c0d0e0f10"
//  raw/rf69/8686-42/24 "040098038018120102030405060708090a0b0c0d0e0f1011"

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>

#include "util.h"
#include "fileaccess.h"
#define BOOT_DATA_MAX 30
#include "bootserver.h"

#include <wiringPi.h>
#include <wiringPiSPI.h>

#include <mosquittopp.h>

// uncomment this on a Raspberry Pi with an ancient version of WiringPi
//#define mosqpp mosquittopp

#define DEBUG   1             // prints all incoming packets to stdout if set
#define NAME    "rf73"        // name of this client, also used in topic
#define SERVER  "127.0.0.1"   // which MQTT server to connected to

// fixed configuration settings for now
#define RF_CHANNEL  23

class MyMqtt : public mosqpp::mosquittopp {
public:
    MyMqtt () : mosqpp::mosquittopp (NAME) { mosqpp::lib_init(); }
    virtual void on_connect (int rc) { printf("connected %d\n", rc); }
    virtual void on_disconnect () { printf("disconnected\n"); }
    virtual void on_message (const struct mosquitto_message* msg);
};

MyMqtt mqtt;
char myTopic [20];

template< int N >
class Pin {
public:
    operator int ()             { return digitalRead(N); }
    void operator = (int value) { digitalWrite(N, value); }
    void setInput ()            { pinMode(N, INPUT); }
    void setOutput ()           { pinMode(N, OUTPUT); }
    void toggle ()              { digitalWrite(N, !digitalRead(N)); }
    int pin ()                  { return N; }
};

#include "spi.h"
#define RFM73 0 // use RFM70
#include "rf73.h"

RF73<SpiDev1,6> rf;

void MyMqtt::on_message (const struct mosquitto_message* msg) {
    const uint8_t* payload = (const uint8_t*) msg->payload;
    uint8_t hdr = (payload[0] & 0x3F) | (payload[1] & 0xC0);
    rf.send(hdr, payload + 2, msg->payloadlen - 2);
}

const char* filePath;

class MyFileAccess : public FileAccess {
public:
    MyFileAccess () : FileAccess (filePath) {}
};

int main (int argc, const char** argv) {
    if (argc < 2) {
        fprintf(stderr, "Usage: rf73bridge <filepathprefix>\n");
        return 1;
    }

    filePath = argv[1];
    sprintf(myTopic, "raw/%s/%d", NAME, RF_CHANNEL);
    printf("\n[rf73bridge] %s @ %s using: %s*\n", myTopic, SERVER, filePath);

    //wiringPiSetup();
    //if (wiringPiSPISetup (1, 4000000) < 0) {
    //    printf("Can't open the SPI bus: %d\n", errno);
    //    return 1;
    //}

    printf("init %d\n", rf.init(RF_CHANNEL));
    //rf.encrypt("mysecret");
    //rf.txPower(15); // 0 = min .. 31 = max

    BootServer<MyFileAccess> server;

    mqtt.connect(SERVER);
    mqtt.subscribe(0, myTopic);

    struct {
        int16_t afc;
        uint8_t rssi;
        uint8_t lna;
        uint8_t buf [32];
    } rx;

    while (true) {
        int len = rf.receive(rx.buf, sizeof rx.buf);
        if (len >= 0) {
#if DEBUG
            printf("OK ");
            for (int i = 0; i < len; ++i)
                printf("%02x", rx.buf[i]);
            //printf(" (%d%s%d:%d)\n",
            //        rf.rssi, rf.afc < 0 ? "" : "+", rf.afc, rf.lna);
            printf("\n");
#endif

            if (rx.buf[1] == 0xC0) {
                BootReply reply;
                int len2 = server.request(rx.buf + 2, len - 2, &reply);
                if (len2 < 0) {
                    printf("ignoring %d -> %d bytes\n", len - 2, len2);
                } else {
                    if (len - 2 != sizeof (FetchRequest) ||
                            len2 != sizeof (FetchReply))
                        printf("sending %d -> %d bytes\n", len - 2, len2);
                    rf.send(0xC0, (const uint8_t*) &reply + 2, len2);
                }
            }

            //rx.afc = rf.afc;
            //rx.rssi = rf.rssi;
            //rx.lna = rf.lna;

            // the topic includes frequency, net group, and origin node id
            char topic [30];
            sprintf(topic, "%s/%d", myTopic, rx.buf[1] & 0x3F);

            // construct a JSON-compatible hex string representation
            //
            // the format is:
            //      2-byte AFC value (little-endian)
            //      1-byte RSSI (raw, 0..255, as in RFM69)
            //      1-byte LNA
            // followed by actual receive data:
            //      1-byte destination (6 bits) and parity (2 bits)
            //      1-byte origin (6 bits) and header flags (2 bits)
            //      ... actual payload data

            char hex [2 * sizeof rx + 3];
            for (int i = 0; i < 4 + len; ++i)
                sprintf(hex+1+2*i, "%02x", ((const uint8_t*) &rx)[i]);
            hex[0] = '"';
            hex[9+2*len] = '"';

            mqtt.publish(0, topic, 10+2*len, (const uint8_t*) hex);
        }

        //chThdYield();
        delay(1);
    }
}
