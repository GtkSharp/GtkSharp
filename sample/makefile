CSC=/cygdrive/c/windows/microsoft.net/framework/v1.0.2914/csc.exe

all:
	@echo "You must use 'make windows' or 'make unix'."
	@echo "'make unix' is broken for now."

windows:
	$(CSC) /unsafe /out:gtk-hello-world.exe /r:../gtk/gtk-sharp.dll /recurse:*.cs

unix:
	@echo "'make unix' is broken for now."
