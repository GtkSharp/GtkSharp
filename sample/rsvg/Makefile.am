assemblies = ../../glib/glib-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../gdk/gdk-sharp.dll ../../gtk/gtk-sharp.dll ../../art-sharp.dll ../../rsvg/rsvg-sharp.dll
references = $(addprefix /r:, $(assemblies))

if ENABLE_RSVG
TARGETS = svghelloworld.exe
else
TARGETS = 
endif
CLEANFILES = $(TARGETS)
noinst_SCRIPTS = $(TARGETS)
EXTRA_DIST = svghelloworld.cs sample.svg

SOURCES = $(srcdir)/svghelloworld.cs
svghelloworld.exe: $(SOURCES) $(assemblies)
	$(CSC) /out:svghelloworld.exe $(SOURCES) $(references)

