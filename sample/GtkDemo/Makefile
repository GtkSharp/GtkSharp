
CSC = mcs

DLLS = /r:gtk-sharp.dll /r:glib-sharp.dll /r:pango-sharp.dll /r:gdk-sharp.dll /r:System.Drawing.dll

all: *.cs
	$(CSC) -g /out:GtkDemo.exe *.cs $(DLLS)

clean:
	rm -f GtkDemo.exe
