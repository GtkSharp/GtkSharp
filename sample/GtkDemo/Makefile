
CSC = mcs

DLLS = /r:gtk-sharp /r:glib-sharp /r:pango-sharp /r:gdk-sharp /r:System.Drawing

all: *.cs
	$(CSC) -g /out:GtkDemo.exe *.cs $(DLLS)

clean:
	rm -f GtkDemo.exe
