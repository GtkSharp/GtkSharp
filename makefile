NATIVE_DIRS = glue parser

DIRS=generator api glib pango atk gdk gtk glade gnome sample
ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.3705/csc.exe
MCS=mcs

all: linux

windows:
	for i in $(DIRS); do					\
		CSC=$(CSC) $(MAKE) -C $$i windows || exit 1;	\
	done;

unix:
	@echo "'make unix' is broken for now."

linux: native binding

binding:
	for i in $(DIRS); do				\
		MCS="$(MCS)" $(MAKE) -C $$i || exit 1;\
	done;

native:
	for i in $(NATIVE_DIRS); do			\
		$(MAKE) -C $$i || exit 1;		\
	done

clean:
	for i in $(NATIVE_DIRS) $(DIRS); do		\
		$(MAKE) -C $$i clean || exit 1;		\
	done;

distclean: clean
	for i in $(NATIVE_DIRS); do			\
		$(MAKE) -C $$i distclean || exit 1;	\
	done
	for i in $(DIRS); do				\
		rm -f $$i/Makefile;			\
	done
	rm -f config.cache config.h config.log config.status libtool

maintainer-clean: distclean
	rm -f aclocal.m4 config.guess config.h.in config.sub
	rm -f configure install-sh ltmain.sh missing
	rm -f mkinstalldirs stamp-h glue/Makefile.in

install: install-native install-binding

install-binding:
	for i in $(DIRS); do				\
		$(MAKE) -C $$i install || exit 1;	\
	done

install-native:
	for i in $(NATIVE_DIRS); do			\
		$(MAKE) -C $$i install || exit 1;	\
	done
