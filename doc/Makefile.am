SUBDIRS = updater 

ASSEMBLER = monodoc --assemble
UPDATER = $(RUNTIME) updater/updater.exe

if ENABLE_GLADE
GLADE_DIR = glade
else
GLADE_DIR =
endif

if ENABLE_GNOME
GNOME_DIR = art gnome gconf/GConf gconf/GConf.PropertyEditors
else
GNOME_DIR =
endif

if ENABLE_GTKHTML
GTKHTML_DIR = gtkhtml
else
GTKHTML_DIR =
endif

if ENABLE_GDA
GDA_DIR = gda
else
GDA_DIR =
endif

if ENABLE_GNOMEDB
GNOMEDB_DIR = gnomedb
else
GNOMEDB_DIR =
endif

if ENABLE_RSVG
RSVG_DIR = rsvg
else
RSVG_DIR =
endif

if ENABLE_VTE
VTE_DIR = vte
else
VTE_DIR =
endif

UPDATE_DIRS=glib pango atk gdk gtk glade $(GNOME_DIR) $(GDA_DIR) $(GNOMEDB_DIR) $(RSVG_DIR) $(GTKHTML_DIR) $(VTE_DIR)
NAMESPACES=GLib Pango Atk Gdk Gtk Glade Art Gnome Gda GnomeDb GConf GConf.PropertyEditors Rsvg Vte

assemble: gtk-sharp-docs.zip gtk-sharp-docs.tree

gtk-sharp-docs.zip gtk-sharp-docs.tree: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(ASSEMBLER) --ecma $(srcdir)/en -o gtk-sharp-docs

update:
	for i in $(UPDATE_DIRS); do                    		\
		cp ../$$i/*.dll updater;				\
		$(UPDATER) ../$$i/*.dll -o ./en || exit 1;	\
        done

CLEANFILES = gtk-sharp-docs.zip gtk-sharp-docs.tree

EXTRA_DIST =			\
	gtk-sharp-docs.source

dist-hook:
	mkdir -p $(distdir)/en
	cp $(srcdir)/en/*.xml $(distdir)/en/
	for i in $(NAMESPACES); do				\
		mkdir -p $(distdir)/en/$$i;			\
		cp $(srcdir)/en/$$i/*.xml $(distdir)/en/$$i;	\
	done


push:
	scp gtk-sharp-docs* miguel@www.go-mono.com:/mono/lib/monodoc/sources
