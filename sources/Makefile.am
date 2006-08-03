EXTRA_DIST =				\
	README				\
	gtk-sharp-2.10-sources.xml	\
	gtkclipboard.patch 		\
	gtk_tree_model_signal_fix.patch \
	gtk_tree_model_signal_fix-2.10.patch


GTK_2_10_DOWNLOADS = \
	ftp://ftp.gtk.org/pub/gtk/v2.10/pango-1.12.3.tar.bz2                           	\
	ftp://ftp.gtk.org/pub/gtk/v2.8/atk-1.10.3.tar.bz2                              	\
	ftp://ftp.gtk.org/pub/gtk/v2.10/gtk+-2.10.0.tar.bz2				\
	http://ftp.gnome.org/pub/GNOME/desktop/2.6/2.6.0/sources/libglade-2.3.6.tar.bz2

api: api-2.10

api-2.10:
	PATH=../parser:$$PATH $(RUNTIME) ../parser/gapi-parser.exe gtk-sharp-2.10-sources.xml

get-source-code: get-2.10-sources 

get-2.10-sources:
	for i in $(GTK_2_10_DOWNLOADS); do                  \
		wget $$i --output-document=- | tar -xj ;   \
	done;
	ln -f -s gtkfilechooserprivate.h gtk+-2.10.0/gtk/gtkfilechooserpriv.h
	patch -p0 gtk+-2.10.0/gtk/gtktreemodel.c < gtk_tree_model_signal_fix-2.10.patch
	echo "typedef struct _GtkClipboard GtkClipboard;" >> gtk+-2.10.0/gtk/gtkclipboard.h
	echo "typedef struct _GtkClipboardClass GtkClipboardClass;" >> gtk+-2.10.0/gtk/gtkclipboard.h

