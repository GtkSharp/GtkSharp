EXTRA_DIST =					\
	README					\
	sources.xml				\
	gtk_tree_model_signal_fix.patch

MAJ=2.24
PT=2.24.0
TARGET_GTK_VERSION=2.14.3

GTK_DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/platform/$(MAJ)/$(PT)/sources/glib-2.18.1.tar.bz2		\
	http://ftp.gnome.org/pub/GNOME/platform/$(MAJ)/$(PT)/sources/pango-1.22.0.tar.bz2		\
	http://ftp.gnome.org/pub/GNOME/platform/$(MAJ)/$(PT)/sources/atk-1.24.0.tar.bz2			\
	http://ftp.gnome.org/pub/GNOME/platform/$(MAJ)/$(PT)/sources/gtk+-$(TARGET_GTK_VERSION).tar.bz2	\
	http://ftp.gnome.org/pub/GNOME/platform/$(MAJ)/$(PT)/sources/libglade-2.6.3.tar.bz2

api: 
	PATH=../parser:$$PATH $(RUNTIME) ../parser/gapi-parser.exe sources.xml

get-source-code:
	for i in $(GTK_DOWNLOADS); do                  \
		wget $$i --output-document=- | tar -xj ;   \
	done;
	ln -f -s gtkfilechooserprivate.h gtk+-$(TARGET_GTK_VERSION)/gtk/gtkfilechooserpriv.h
	patch -p0 gtk+-$(TARGET_GTK_VERSION)/gtk/gtktreemodel.c < gtk_tree_model_signal_fix.patch
	echo "typedef struct _GtkClipboard GtkClipboard;" >> gtk+-$(TARGET_GTK_VERSION)/gtk/gtkclipboard.h
	echo "typedef struct _GtkClipboardClass GtkClipboardClass;" >> gtk+-$(TARGET_GTK_VERSION)/gtk/gtkclipboard.h

