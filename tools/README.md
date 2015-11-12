Embello Tools
=============

Installing Go on ARM
--------------------

Some of the tools here are written in [Go](http://golang.org) and require the Go toolchain to be installed in order to complile
them. The tools can be installed on Linux, OS-X and Windows to run on x86 and ARM. For x86 and i386 installas download a binary
package starting from http://golang.org/doc/install, for ARM installs visit http://dave.cheney.net/unofficial-arm-tarballs for
info and follow these instructions (tested on a BBB running Ubuntu 14.04, on an RPi replace `armv7-1` by `armv6-1` below):
```
sudo -s
cd /usr/local
curl http://dave.cheney.net/paste/go1.4.linux-arm~multiarch-armv7-1.tar.gz | tar zxf -
echo "export PATH=$PATH:/usr/local/go/bin" >>/etc/profile.d/go.sh
exit
echo 'export GOPATH=$HOME/go' >>~/.profile
echo 'export PATH=$PATH:$GOPATH/bin' >>~/.profile
mkdir $HOME/go
```
Now make sure Go functions:
```
source /etc/profile.d/go.sh # not necessary the next time you log in
source ~/.profile           # not necessary the next time you log in
go version
```
This should print out `go version go1.4 linux/arm`.

If you later need to upgrade Go to a later version, my recommendation would be:
```
sudo -s
cd /usr/local
mv go `cat go/VERSION`
curl http://dave.cheney.net/paste/go1.X.linux-arm~multiarch-armv7-1.tar.gz | tar zxf -
exit
```

Above "arm7-1" confirmed to also work on Odroid-C1, 2015-02-09.
