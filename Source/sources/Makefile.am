EXTRA_DIST =					\
	README					\
	sources.xml				\
	gtk_tree_model_signal_fix.patch

TARGET_GTK_VERSION=3.22.6
TARGET_GTK_API=3.22

GTK_DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/sources/glib/2.50/glib-2.50.2.tar.xz 			\
	http://ftp.gnome.org/pub/GNOME/sources/pango/1.40/pango-1.40.3.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/atk/2.22/atk-2.22.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/gdk-pixbuf/2.36/gdk-pixbuf-2.36.2.tar.xz	\
	http://ftp.gnome.org/pub/GNOME/sources/gtk+/$(TARGET_GTK_API)/gtk+-$(TARGET_GTK_VERSION).tar.xz

api: 
	PATH=../parser:$$PATH $(RUNTIME) ../parser/gapi-parser.exe sources.xml

get-source-code:
	for i in $(GTK_DOWNLOADS); do                  \
		wget $$i --output-document=- | tar -xJ ;   \
	done;
	ln -f -s gtkfilechooserprivate.h gtk+-$(TARGET_GTK_VERSION)/gtk/gtkfilechooserpriv.h
	patch -p1 -d gtk+-$(TARGET_GTK_VERSION) < patches/gtk_tree_model_signal_fix.patch
	echo "typedef struct _GtkClipboard GtkClipboard;" >> gtk+-$(TARGET_GTK_VERSION)/gtk/gtkclipboard.h
	echo "typedef struct _GtkClipboardClass GtkClipboardClass;" >> gtk+-$(TARGET_GTK_VERSION)/gtk/gtkclipboard.h
	patch -p0 gtk+-$(TARGET_GTK_VERSION)/gtk/gtktextattributes.h < patches/gtktextattributes-gi-scanner.patch
	patch -p0 glib-2.50.2/gio/gwin32registrykey.h < patches/gwin32registrykey-little-endian.patch
