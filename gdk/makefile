MCS=mcs

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /target:library /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /out:gdk-sharp.dll /recurse:*.cs

linux:
	$(MCS) --unsafe --target library -r ../glib/glib-sharp.dll -r ../pango/pango-sharp.dll -r ../atk/atk-sharp.dll -o gdk-sharp.dll --recurse *.cs

unix:
	@echo "'make unix' is broken for now."
