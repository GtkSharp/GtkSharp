docsdir = $(datadir)/doc/gtk-sharp/xml
docs_DATA = gtk-sharp-docs.source

ASSEMBLER = monodoc --assemble
UPDATER = monodoc --update

DIRS=glib pango atk gdk gtk glade art gnome gda gnomedb gconf/GConf gconf/GConf.PropertyEditors rsvg gtkhtml vte
NAMESPACES=GLib Pango Atk Gdk Gtk Glade Art Gnome Gda GnomeDb GConf GConf.PropertyEditors Rsvg Vte

assemble: gtk-sharp-docs.zip gtk-sharp-docs.tree

gtk-sharp-docs.zip gtk-sharp-docs.tree: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(ASSEMBLER) --ecma $(srcdir)/en -o gtk-sharp-docs

update:
	for i in $(DIRS); do                    		\
		$(UPDATER) ../$$i/*.dll -o ./en || exit 1;	\
        done

sources =			\
	Tool.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

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

install-data-local:
	mkdir -p $(DESTDIR)$(docsdir)/en
	cp $(srcdir)/en/*.xml $(DESTDIR)$(docsdir)/en;
	for i in $(NAMESPACES); do					\
		mkdir -p $(DESTDIR)$(docsdir)/en/$$i;			\
		cp $(srcdir)/en/$$i/*.xml $(DESTDIR)$(docsdir)/en/$$i;	\
	done

uninstall-local:
	rm -rf $(DESTDIR)$(docsdir)/en

