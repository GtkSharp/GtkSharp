DIRS=generator glib pango gdk gtk sample
ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	for i in $(DIRS); do				\
		(cd $$i; CSC=$(CSC) make windows) || exit 1;\
	done;

unix:
	@echo "'make unix' is broken for now."
