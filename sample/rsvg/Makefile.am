references = /r:../../glib/glib-sharp.dll /r:../../pango/pango-sharp.dll /r:../../atk/atk-sharp.dll /r:../../gdk/gdk-sharp.dll /r:../../gtk/gtk-sharp.dll /r:../../glade/glade-sharp.dll /r:../../art/art-sharp.dll /r:../../gnome/gnome-sharp.dll /r:../../rsvg/rsvg-sharp.dll

TARGETS = svghelloworld.exe
CLEANFILES = $(TARGETS)
noinst_SCRIPTS = $(TARGETS)
EXTRA_DIST = svghelloworld.cs sample.svg

SOURCES = $(srcdir)/svghelloworld.cs
svghelloworld.exe: $(SOURCES)
	$(CSC) /out:svghelloworld.exe $(SOURCES) $(references)

