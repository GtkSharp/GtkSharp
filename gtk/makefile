all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /target:library /r:../glib/glib-sharp.dll /r:../gdk/gdk-sharp.dll /out:gtk-sharp.dll /recurse:*.cs

unix:
	@echo "'make unix' is broken for now."
