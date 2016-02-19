# not used yet
test:
	go test

# build msend for local use
msend:
	go build -o $@

# build msend for several platforms and compress for release
builds:
	@ rm -rf $@; mkdir $@
	@ echo Re-building binaries:
	@ echo "  MacOSX 64-bit"
	@ GOOS=darwin GOARCH=amd64 go build -o $@/msend-darwin-amd64
	@ echo "  ARMv6 32-bit"
	@ GOOS=linux GOARCH=arm GOARM=6 go build -o $@/msend-linux-arm
	@ echo "  Linux 32-bit"
	@ GOOS=linux GOARCH=386 go build -o $@/msend-linux-386
	@ echo "  Linux 64-bit"
	@ GOOS=linux GOARCH=amd64 go build -o $@/msend-linux-amd64
	@ gzip $@/msend-*
	@ echo; ls -l builds/*; echo

clean:
	rm -f msend builds

.PHONY: test msend builds clean

# vim: set noexpandtab :
