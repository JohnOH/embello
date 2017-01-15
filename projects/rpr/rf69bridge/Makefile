# comment this out for older gcc's
CXXFLAGS += -std=gnu++11

CXXFLAGS += -I../../../lib/arch-raspi -I../../../lib/driver
CXXFLAGS += -I../../../tools/jeeboot/include
LDLIBS = -lwiringPi -lwiringPiDev -lpthread -lmosquittopp

all: rf69bridge
	sudo ./rf69bridge files/

clean:
	rm -f *.o rf69bridge
