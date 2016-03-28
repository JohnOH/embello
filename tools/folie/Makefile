# not used yet
test:
	go test

# build folie for local use
folie:
	go build -o $@

# build folie for several platforms and compress for release
builds:
	@ rm -rf $@; mkdir $@
	@ echo Re-building binaries:
	@ echo "  MacOSX 64-bit"
	@ GOOS=darwin GOARCH=amd64 go build -o $@/folie-darwin-amd64
	@ echo "  Windows 64-bit"
	@ GOOS=windows GOARCH=amd64 go build -o $@/folie-windows-amd64
	@ echo "  ARMv6 32-bit"
	@ GOOS=linux GOARCH=arm GOARM=6 go build -o $@/folie-linux-arm
	@ echo "  Linux 32-bit"
	@ GOOS=linux GOARCH=386 go build -o $@/folie-linux-386
	@ echo "  Linux 64-bit"
	@ GOOS=linux GOARCH=amd64 go build -o $@/folie-linux-amd64
	@ gzip $@/folie-*
	@ echo; ls -l builds/*; echo

clean:
	rm -f folie builds

.PHONY: test folie builds clean

# vim: set noexpandtab :
