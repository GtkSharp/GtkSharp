DIRS=glue generator glib pango atk gdk gtk sample
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

linux: 
	for i in $(DIRS); do				\
		(cd $$i; MCS="$(MCS)" make) || exit 1;\
	done;

install:
	for i in $(DIRS); do				\
		(cd $$i; make install) || exit 1;	\
	done;

