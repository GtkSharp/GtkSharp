all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /target:library /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll /out:gtk-sharp.dll /recurse:*.cs

linux:
	mcs --unsafe --target library -r ../glib/glib-sharp.dll -r ../pango/pango-sharp.dll -r ../atk/atk-sharp.dll -r ../gdk/gdk-sharp.dll -o gtk-sharp.dll --recurse *.cs

unix:
	@echo "'make unix' is broken for now."

