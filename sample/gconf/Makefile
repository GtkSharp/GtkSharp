SOURCES = \
	Settings.cs \
	main.cs \
	#

MCS = mcs
GCONFDIR=../../gconf

all: sample.exe

sample.exe: sample.schema sample.glade $(SOURCES)
	$(MCS) /out:sample.exe $(SOURCES) /r:$(GCONFDIR)/GConf/gconf-sharp.dll /r:$(GCONFDIR)/GConf.PropertyEditors/gconf-sharp-peditors.dll /r:gtk-sharp /r:glade-sharp /r:gnome-sharp /r:System.Drawing /resource:sample.glade

Settings.cs: sample.schema
	MONO_PATH=$(top_builddir)/gconf/GConf/gconf-sharp.dll mono $(GCONFDIR)/tools/gconfsharp-schemagen.exe Sample sample.schema > Settings.cs

install: all
	GCONF_CONFIG_SOURCE="" gconftool-2 --makefile-install-rule sample.schema

clean:
	rm -f sample.exe Settings.cs
