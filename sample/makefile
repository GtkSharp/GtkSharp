all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /out:gtk-hello-world.exe /r:../glib/glib-sharp.dll /r:../gtk/gtk-sharp.dll /recurse:*.cs

unix:
	@echo "'make unix' is broken for now."
