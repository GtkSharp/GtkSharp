ASSEMBLIES = 					\
	lib/glib-sharp.dll			\
	lib/gio-sharp.dll			\
	lib/pango-sharp.dll			\
	lib/atk-sharp.dll			\
	lib/gdk-sharp.dll			\
	lib/gtk-sharp.dll			\
	lib/gtk-dotnet.dll

UPDATER = $(MDOC) update --out=en -pretty --fno-assembly-versions --since="Gtk# 3.0" $(ASSEMBLIES)

if ENABLE_MONODOC
SOURCESDIR=$(prefix)/lib/monodoc/sources
TARGETS=gtk-sharp-3-docs.zip gtk-sharp-3-docs.tree gtk-sharp-3-docs.source
else
SOURCESDIR=$(datadir)
TARGETS=
endif

monodocdir=$(SOURCESDIR)
monodoc_DATA=$(TARGETS)

assemble: gtk-sharp-3-docs.zip gtk-sharp-3-docs.tree

gtk-sharp-3-docs.tree: gtk-sharp-3-docs.zip

gtk-sharp-3-docs.zip: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(MDOC) assemble -o gtk-sharp-3-docs $(srcdir)/en

get-assemblies:
	mkdir -p lib
	cp $(top_builddir)/*/*.dll lib
	cp $(top_builddir)/*/*.dll.config lib

update: get-assemblies
	$(MDOC) update --out=en --fno-assembly-versions --since="Gtk# 3.0" $(ASSEMBLIES)

update-delete: get-assemblies
	$(MDOC) update --out=en --fno-assembly-versions --delete --since="Gtk# 3.0" $(ASSEMBLIES)

CLEANFILES = gtk-sharp-3-docs.zip gtk-sharp-3-docs.tree lib

EXTRA_DIST =			\
	gtk-sharp-3-docs.source

NAMESPACES=GLib Pango Atk Gdk Gtk Gtk.DotNet

dist-hook:
	mkdir -p $(distdir)/en
	cp $(srcdir)/en/*.xml $(distdir)/en/
	for i in $(NAMESPACES); do				\
		mkdir -p $(distdir)/en/$$i;			\
		cp $(srcdir)/en/$$i/*.xml $(distdir)/en/$$i;	\
	done

