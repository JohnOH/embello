CXXFLAGS += -I../../../lib/arch-raspi -I../../../lib/driver
LDLIBS = -lwiringPi -lwiringPiDev -lpthread -lmosquittopp

all: rf69mqtt
	sudo ./rf69mqtt

clean:
	rm -f *.o rf69mqtt
