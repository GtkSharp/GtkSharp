EXTRA_DIST =					\
	README					\
	sources.xml				\
	gtk_tree_model_signal_fix.patch

TARGET_GTK_VERSION=3.20.4
TARGET_GTK_API=3.20

GTK_DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/sources/glib/2.48/glib-2.48.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/pango/1.40/pango-1.40.1.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/atk/2.20/atk-2.20.0.tar.xz			\
	http://ftp.gnome.org/pub/GNOME/sources/gdk-pixbuf/2.34/gdk-pixbuf-2.34.0.tar.xz	\
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
	patch -p0 gtk+-$(TARGET_GTK_VERSION)/gtk/gtktextview.h < gtk_text_view_public.patch 
	patch -p0 glib-2.48.0/gio/gwin32registrykey.h < gwin32registrykey-little-endian.patch

