# Generated automatically from Makefile.in by configure.
MCS=mcs
DESTDIR=

all: linux

windows:
	$(CSC) /unsafe /target:library /r:../glib/glib-sharp.dll /out:gnomeprint-sharp.dll /recurse:*.cs

linux: gnomeprint-sharp.dll

gnomeprint-sharp.dll: generated/*.cs
	$(MCS) --unsafe --target library -r glib-sharp.dll -o gnomeprint-sharp.dll --recurse '*.cs'

clean:
	rm -f *.dll
	rm -rf generated

install: all
	cp gst-sharp.dll $(DESTDIR)/usr/local//lib || exit 1

