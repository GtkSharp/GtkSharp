ASSEMBLIES = 					\
	lib/glib-sharp.dll			\
	lib/pango-sharp.dll			\
	lib/atk-sharp.dll			\
	lib/gdk-sharp.dll			\
	lib/gtk-sharp.dll			\
	lib/glade-sharp.dll			\
	lib/gnome-vfs-sharp.dll			\
	lib/art-sharp.dll			\
	lib/gnome-sharp.dll			\
	lib/gconf-sharp.dll			\
	lib/gconf-sharp-peditors.dll		\
	lib/gtkhtml-sharp.dll			\
	lib/rsvg2-sharp.dll			\
	lib/vte-sharp.dll			\
	lib/gtk-dotnet.dll

UPDATER = $(MDOC) update --out=en -pretty --fno-assembly-versions --since="Gtk# 2.14" $(ASSEMBLIES)

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
	$(MDOC) assemble -o gtk-sharp-docs $(srcdir)/en

get-assemblies:
	echo "assumes gnome-sharp and gtk-sharp checkouts in same parent"
	mkdir -p lib
	cp $(top_builddir)/*/*.dll lib
	cp $(top_builddir)/*/*.dll.config lib
	cp $(top_builddir)/../gnome-sharp/*/*.dll lib
	cp $(top_builddir)/../gnome-sharp/*/*.dll.config lib
	cp $(top_builddir)/../gnome-sharp/gconf/*/*.dll lib
	cp $(top_builddir)/../gnome-sharp/gconf/*/*.dll.config lib
	cp $(top_builddir)/../gnome-desktop-sharp/*/*.dll lib
	cp $(top_builddir)/../gnome-desktop-sharp/*/*.dll.config lib

update: get-assemblies
	$(MDOC) update --out=en --fno-assembly-versions --since="Gtk# 2.14" $(ASSEMBLIES)

update-delete: get-assemblies
	$(MDOC) update --out=en --fno-assembly-versions --delete --since="Gtk# 2.14" $(ASSEMBLIES)

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
	scp gtk-sharp-docs* root@www.go-mono.com:/usr/lib/monodoc/sources/

