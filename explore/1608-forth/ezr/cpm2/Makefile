simh = ../simh2
boot = -b $(simh)/SLOAD.COM -b $(simh)/BDOS22.COM -b $(simh)/MINBIOS.COM
coms = HEXSAVE.COM EZFLASH.COM FMT360.COM XMODEM.COM

all: disk disk2

disk:
	@rm -f disk.img
	@mkfs.cpm -f memotech-type50 $(boot) disk.img
	@cpmcp -f memotech-type50 disk.img $(simh)/HEXSAVE.COM 0:
	@go run bin2fs.go
	@cpmls -f memotech-type50 -l disk.img

disk2:
	@rm -f disk2.img
	@mkfs.cpm -f memotech-type50 $(boot) disk2.img
	@for i in $(coms) utils/*; do \
	    cpmcp -f memotech-type50 disk2.img $(simh)/$$i 0: ; done
	@cpmcp -f memotech-type50 -t disk2.img XMODEM.CFG 0:
	@ go run bin2fs.go disk2.img disk2.fs
	@cpmls -f memotech-type50 -l disk2.img
