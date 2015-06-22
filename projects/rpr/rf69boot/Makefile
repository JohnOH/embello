CXXFLAGS += -I../../../lib/arch-raspi -I../../../lib/driver
CXXFLAGS += -I../../../../jeeboot/include
LDLIBS = -lwiringPi -lwiringPiDev -lpthread

all: rf69boot
	sudo ./rf69boot

clean:
	rm -f *.o rf69boot
