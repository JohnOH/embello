CXXFLAGS += -I../../../lib/arch-raspi -I../../../lib/driver
CXXFLAGS += -I../../../tools/jeeboot/include
LDLIBS = -lwiringPi -lwiringPiDev -lpthread

all: rf69boot
	sudo ./rf69boot files/

clean:
	rm -f *.o rf69boot
