if ENABLE_GNOME
TARGET = sample.exe
else
TARGET =
endif

FILES = 		\
	Settings.cs 	\
	$(srcdir)/main.cs

SCHEMA = $(srcdir)/sample.schema
RESOURCES = /resource:$(srcdir)/sample.glade
ASSEMBLIES = ../../glib/glib-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../art/art-sharp.dll ../../gdk/gdk-sharp.dll $(GCONFDIR)/GConf/gconf-sharp.dll ../../gtk/gtk-sharp.dll ../../gnome/gnome-sharp.dll ../../glade/glade-sharp.dll $(GCONFDIR)/GConf.PropertyEditors/gconf-sharp-peditors.dll
REFERENCES = $(addprefix /r:, $(ASSEMBLIES))

top_builddir=../..
GCONFDIR=$(top_builddir)/gconf

noinst_SCRIPTS = $(TARGET)

sample.exe: $(SCHEMA) $(srcdir)/sample.glade $(FILES) $(ASSEMBLIES)
	$(CSC) /out:sample.exe $(FILES) $(REFERENCES) $(RESOURCES)

Settings.cs: $(SCHEMA)
	MONO_PATH=$(top_builddir)/gconf/GConf/gconf-sharp.dll:${MONO_PATH} $(RUNTIME) $(GCONFDIR)/tools/gconfsharp-schemagen.exe Sample $(SCHEMA) > Settings.cs

install-hook: 
	GCONF_CONFIG_SOURCE="" gconftool-2 --makefile-install-rule $(SCHEMA)

CLEANFILES = sample.exe Settings.cs

EXTRA_DIST = main.cs sample.schema sample.glade sample.gladep
