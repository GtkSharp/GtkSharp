EXTRA_DIST =				\
	README				\
	gtk-sharp-sources.xml		\
	gda.patch			\
	gnomedb.patch			\
	atkhyperlink.patch		\
	gtk_tree_model_signal_fix.patch \
	gtkhtml-font-style-enum.patch


DOWNLOADS = \
	ftp://ftp.gtk.org/pub/gtk/v2.4/pango-1.4.0.tar.gz                                       \
	ftp://ftp.gtk.org/pub/gtk/v2.4/atk-1.6.0.tar.gz                                         \
	ftp://ftp.gtk.org/pub/gtk/v2.4/gtk+-2.4.1.tar.gz                                        \
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/gnome-vfs-2.6.0.tar.gz		\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libgnome-2.6.0.tar.gz       	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libgnomecanvas-2.6.0.tar.gz 	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libgnomeui-2.6.0.tar.gz     	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libgnomeprint-2.6.0.tar.gz	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libgnomeprintui-2.6.0.tar.gz	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libglade-2.3.6.tar.gz		\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libart_lgpl-2.3.16.tar.gz	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/librsvg-2.6.4.tar.gz         	\
	http://ftp.gnome.org/pub/GNOME/sources/libgda/1.0/libgda-1.0.0.tar.gz			\
	http://ftp.gnome.org/pub/GNOME/sources/libgnomedb/1.0/libgnomedb-1.0.0.tar.gz		\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/vte-0.11.10.tar.gz \
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/gnome-panel-2.6.0.tar.gz
	
GTKHTML_SOURCEFILES = \
        gtkhtml-3.0.10/src/gtkhtml.c \
        gtkhtml-3.0.10/src/gtkhtml.h \
        gtkhtml-3.0.10/src/gtkhtml-types.h \
        gtkhtml-3.0.10/src/gtkhtml-enums.h \
        gtkhtml-3.0.10/src/gtkhtml-embedded.c \
        gtkhtml-3.0.10/src/gtkhtml-embedded.h \
        gtkhtml-3.0.10/src/gtkhtml-stream.c \
        gtkhtml-3.0.10/src/gtkhtml-stream.h

api:
	PATH=../parser:$$PATH $(RUNTIME) ../parser/gapi-parser.exe gtk-sharp-sources.xml

get-gtkhtml-code:
	wget http://ftp.gnome.org/pub/GNOME/sources/gtkhtml/3.0/gtkhtml-3.0.10.tar.gz --output-document=- | tar -xz $(GTKHTML_SOURCEFILES)
	patch -p0 < gtkhtml-font-style-enum.patch
	
get-source-code: get-gtkhtml-code
	for i in $(DOWNLOADS); do                          \
		wget $$i --output-document=- | tar -xz ;   \
	done;

	ln -f -s gtkfilechooserprivate.h gtk+-2.4.1/gtk/gtkfilechooserpriv.h
	patch -p0 atk-1.6.0/atk/atkhyperlink.h < atkhyperlink.patch
	patch -p0 gtk+-2.4.1/gtk/gtktreemodel.c < gtk_tree_model_signal_fix.patch
	patch -p0 libgnomedb-1.0.0/libgnomedb/gnome-db-list.c < gnomedb.patch
	patch -p0 libgda-1.0.0/libgda/gda-export.c < gda.patch
