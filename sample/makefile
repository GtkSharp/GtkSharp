all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /out:gtk-hello-world.exe /r:../glib/glib-sharp.dll /r:../gtk/gtk-sharp.dll /r:../gdk/gdk-sharp.dll HelloWorld.cs
	$(CSC) /unsafe /out:button.exe /r:../glib/glib-sharp.dll /r:../gtk/gtk-sharp.dll ButtonApp.cs

unix:
	@echo "'make unix' is broken for now."
