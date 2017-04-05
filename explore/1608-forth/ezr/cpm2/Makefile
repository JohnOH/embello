s   = ../simh2
min = -b $s/RLOAD.COM -b $s/SLOAD.COM -b $s/BDOS22.COM -b $s/MINBIOS.COM
max = -b $s/RLOAD.COM -b $s/SLOAD.COM -b $s/BDOS22.COM -b $s/MAXBIOS.COM
com = HEXSAVE.COM GETFLASH.COM PUTFLASH.COM XMODEM.COM MAPDRIVE.COM

all: disk disk1 disk2

disk:
	@rm -f disk.img
	@mkfs.cpm -f memotech-type50 $(min) disk.img
	@cpmcp -f memotech-type50 disk.img $s/HEXSAVE.COM 0:
	@go run bin2fs.go
	@cpmls -f memotech-type50 -l disk.img

disk1:
	@rm -f disk1.img
	@mkfs.cpm -f memotech-type50 $(max) disk1.img
	@cpmcp -f memotech-type50 disk1.img $s/HEXSAVE.COM 0:
	@go run bin2fs.go disk1.img disk1.fs
	@cpmls -f memotech-type50 -l disk1.img

disk2:
	@rm -f disk2.img
	@mkfs.cpm -f memotech-type50 $(max) disk2.img
	@for i in $(com) "utils/*"; do \
	    cpmcp -f memotech-type50 disk2.img $s/$$i 0: ; done
	@cpmcp -f memotech-type50 -t disk2.img XMODEM.CFG 0:
	@ go run bin2fs.go disk2.img disk2.fs
	@cpmls -f memotech-type50 -l disk2.img
