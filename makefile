DIRS=generator glib pango atk gdk gtk sample
ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))

all:
	@echo "You must use 'make windows' or 'make linux'."
	@echo "'make unix' is broken for now."

windows:
	CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe
	for i in $(DIRS); do				\
		(cd $$i; CSC=$(CSC) make windows) || exit 1;\
	done;

unix:
	@echo "'make unix' is broken for now."

linux:
	for i in $(DIRS); do				\
		(cd $$i; make linux) || exit 1;\
	done;
