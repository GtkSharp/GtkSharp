ASSEMBLIES = 				\
	glib-sharp.dll			\
	pango-sharp.dll			\
	atk-sharp.dll			\
	gdk-sharp.dll			\
	gtk-sharp.dll			\
	glade-sharp.dll			\
	gnome-vfs-sharp.dll		\
	art-sharp.dll			\
	gnome-sharp.dll			\
	gconf-sharp.dll			\
	gconf-sharp-peditors.dll	\
	gtkhtml-sharp.dll		\
	rsvg-sharp.dll			\
	vte-sharp.dll			\
	gtk-dotnet.dll

UPDATE_ASSEMBLIES = $(addprefix -assembly:lib/, $(ASSEMBLIES))

if ENABLE_ALL_OPTIONAL
UPDATER = $(MONODOCER) -path:en $(UPDATE_ASSEMBLIES)
else
UPDATER = echo "All optional assemblies must be built to use the updater." && exit 1
endif

if ENABLE_MONODOC
SOURCESDIR=$(prefix)/lib/monodoc/sources
TARGETS=gtk-sharp-docs.zip gtk-sharp-docs.tree gtk-sharp-docs.source
else
SOURCESDIR=$(datadir)
TARGETS=
endif

monodocdir=$(SOURCESDIR)
monodoc_DATA=$(TARGETS)

assemble: gtk-sharp-docs.zip gtk-sharp-docs.tree

gtk-sharp-docs.zip gtk-sharp-docs.tree: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(MDASSEMBLER) --ecma $(srcdir)/en -o gtk-sharp-docs

get-assemblies:
	mkdir -p lib
	cp $(top_builddir)/*/*.dll lib
	cp $(top_builddir)/*/*.dll.config lib
	cp $(top_builddir)/gconf/*/*.dll lib
	cp $(top_builddir)/gconf/*/*.dll.config lib

update: get-assemblies
	$(UPDATER)

update-delete: get-assemblies
	$(UPDATER) --delete

CLEANFILES = gtk-sharp-docs.zip gtk-sharp-docs.tree lib

EXTRA_DIST =			\
	gtk-sharp-docs.source

NAMESPACES=GLib Pango Atk Gdk Gtk Gtk.DotNet Glade Art Gnome.Vfs Gnome GConf GConf.PropertyEditors Rsvg Vte

dist-hook:
	mkdir -p $(distdir)/en
	cp $(srcdir)/en/*.xml $(distdir)/en/
	for i in $(NAMESPACES); do				\
		mkdir -p $(distdir)/en/$$i;			\
		cp $(srcdir)/en/$$i/*.xml $(distdir)/en/$$i;	\
	done


push:
	scp gtk-sharp-docs* miguel@www.go-mono.com:/mono/lib/monodoc/sources

