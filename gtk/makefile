all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows: defs
	$(CSC) /unsafe /target:library /r:../glib/glib-sharp.dll /r:../gdk/gdk-sharp.dll /out:gtk-sharp.dll /recurse:*.cs

unix:
	@echo "'make unix' is broken for now."

defs:
	../defs-parse.pl < gtk.defs > generated.cs
