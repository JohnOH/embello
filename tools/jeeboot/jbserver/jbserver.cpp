#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "util.h"
#include "fileaccess.h"
#include "bootserver.h"

#include <mosquittopp.h>

#if linux
#define mosqpp mosquittopp
#endif

static const char* filePath;

// TODO hack, to get the constructor called without passing in any args
class MyFileAccess : public FileAccess {
public:
    MyFileAccess () : FileAccess (filePath) {}
};

class MyMqtt : public mosqpp::mosquittopp {
    BootServer<MyFileAccess> server;

public:
    MyMqtt () : mosqpp::mosquittopp ("jeeboot") { mosqpp::lib_init(); }

    virtual void on_connect (int rc) { printf("connected (%d)\n", rc); }
    virtual void on_disconnect () { printf("disconnected\n"); }

    virtual void on_message (const struct mosquitto_message* msg) {
        // this must be large enough to hold any possible reply packet
        BootReply reply;

        int len = server.request(msg->payload, msg->payloadlen, &reply);
        if (len < 0) {
            printf("ignoring %d bytes\n", msg->payloadlen);
            return;
        }

        printf("sending %d bytes\n", len);
        publish(0, "raw/rf69/868-42/oob-out", len, (const uint8_t*) &reply);
    }
};

int main (int argc, const char** argv) {
    if (argc < 2) {
        fprintf(stderr, "Usage: jbserver <filepathprefix> ?mqttserver?\n");
        return 1;
    }

    filePath = argv[1];
    printf("\n[jbserver] using: %s*\n", filePath);

    MyMqtt mqtt;
    mqtt.connect(argc > 2 ? argv[2] : "127.0.0.1");
    mqtt.subscribe(0, "raw/rf69/868-42/oob-in");

    for (;;)
        mqtt.loop(1000);

    return 0;
}
