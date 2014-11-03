EXTRA_DIST =					\
	README					\
	sources.xml				\
	gtk_tree_model_signal_fix.patch

TARGET_GTK_VERSION=3.12.0
TARGET_GTK_API=3.12

GTK_DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/sources/glib/2.40/glib-2.40.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/pango/1.36/pango-1.36.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/atk/2.12/atk-2.12.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/gdk-pixbuf/2.28/gdk-pixbuf-2.28.0.tar.xz	\
	http://ftp.gnome.org/pub/GNOME/sources/gtk+/$(TARGET_GTK_API)/gtk+-$(TARGET_GTK_VERSION).tar.xz

api: 
	PATH=../parser:$$PATH $(RUNTIME) ../parser/gapi-parser.exe sources.xml

get-source-code:
	for i in $(GTK_DOWNLOADS); do                  \
		wget $$i --output-document=- | tar -xJ ;   \
	done;
	ln -f -s gtkfilechooserprivate.h gtk+-$(TARGET_GTK_VERSION)/gtk/gtkfilechooserpriv.h
	patch -p0 gtk+-$(TARGET_GTK_VERSION)/gtk/gtktreemodel.c < gtk_tree_model_signal_fix.patch
	echo "typedef struct _GtkClipboard GtkClipboard;" >> gtk+-$(TARGET_GTK_VERSION)/gtk/gtkclipboard.h
	echo "typedef struct _GtkClipboardClass GtkClipboardClass;" >> gtk+-$(TARGET_GTK_VERSION)/gtk/gtkclipboard.h
	patch -p0 gtk+-$(TARGET_GTK_VERSION)/gtk/gtkselectionprivate.h < gtkselectionprivate-space.patch 
	patch -p0 gtk+-$(TARGET_GTK_VERSION)/gtk/gtktextattributes.h < gtktextattributes-gi-scanner.patch 

