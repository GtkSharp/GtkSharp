NATIVE_DIRS = glue parser

DIRS=generator api glib pango atk gdk art gtk glade gnome sample
ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.3705/csc.exe
MCS=mcs

all: linux

windows:
	for i in $(DIRS); do				\
		(cd $$i; CSC=$(CSC) make windows) || exit 1;\
	done;

unix:
	@echo "'make unix' is broken for now."

linux: native binding

binding:
	for i in $(DIRS); do				\
		(cd $$i; MCS="$(MCS)" make) || exit 1;\
	done;

native:
	for i in $(NATIVE_DIRS); do \
		(cd $$i; make) || exit 1;\
	done

clean:
	for i in $(NATIVE_DIRS) $(DIRS); do	\
		(cd $$i; make clean) || exit 1;	\
	done;

distclean: clean
	for i in $(NATIVE_DIRS); do \
		(cd $$i; make distclean) || exit 1;\
	done
	for i in $(DIRS); do				\
		rm -f $$i/Makefile;			\
	done
	rm -f config.cache config.h config.log config.status libtool

install: install-native install-binding

install-binding:
	for i in $(DIRS); do				\
		(cd $$i; make install) || exit 1;	\
	done;

install-native:
	for i in $(NATIVE_DIRS); do \
		(cd $$i; make install) || exit 1;\
	done

