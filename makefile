DIRS=generator glib pango atk gdk gdk.imaging gtk sample
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
	(cd glue; make) || exit 1;

clean:
	(cd glue; make clean) || exit 1;
	for i in $(DIRS); do				\
		(cd $$i; make clean) || exit 1;	\
	done;

install: install-native install-binding

install-binding:
	for i in $(DIRS); do				\
		(cd $$i; make install) || exit 1;	\
	done;

install-native:
	(cd glue; make install) || exit 1;	\

