SOURCES = 		\
	Settings.cs 	\
	$(srcdir)/main.cs

SCHEMA = $(srcdir)/sample.schema
RESOURCES = /resource:$(srcdir)/sample.glade
REFERENCES = /r:../../glib/glib-sharp.dll /r:../../pango/pango-sharp.dll /r:../../atk/atk-sharp.dll /r:../../art/art-sharp.dll /r:../../gdk/gdk-sharp.dll /r:$(GCONFDIR)/GConf/gconf-sharp.dll /r:../../gtk/gtk-sharp.dll /r:../../gnome/gnome-sharp.dll /r:../../glade/glade-sharp.dll /r:$(GCONFDIR)/GConf.PropertyEditors/gconf-sharp-peditors.dll /r:System.Drawing

top_builddir=../..
GCONFDIR=$(top_builddir)/gconf

noinst_SCRIPTS = sample.exe

sample.exe: $(SCHEMA) $(srcdir)/sample.glade $(SOURCES)
	$(CSC) /out:sample.exe $(SOURCES) $(REFERENCES) $(RESOURCES)

Settings.cs: $(SCHEMA)
	MONO_PATH=$(top_builddir)/gconf/GConf/gconf-sharp.dll:${MONO_PATH} $(RUNTIME) $(GCONFDIR)/tools/gconfsharp-schemagen.exe Sample $(SCHEMA) > Settings.cs

install-hook: 
	GCONF_CONFIG_SOURCE="" gconftool-2 --makefile-install-rule $(SCHEMA)

CLEANFILES = sample.exe Settings.cs

EXTRA_DIST = main.cs sample.schema sample.glade sample.gladep
