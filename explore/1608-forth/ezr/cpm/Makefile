bootfiles = -b ../asm/powerup.bin -b CPM22.BIN -b CBIOS128.BIN

all:
	rm -f disk.img
	mkfs.cpm -f memotech-type50 $(bootfiles) disk.img
	cpmcp -f memotech-type50 disk.img HEXSAVE.COM 0:
	go run bin2fs.go
