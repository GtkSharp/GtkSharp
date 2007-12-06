EXTRA_DIST =				\
	README				\
	gtk-sharp-2.12-sources.xml	\
	gtkclipboard.patch 		\
	gtk_tree_model_signal_fix.patch \
	gtk_tree_model_signal_fix-2.10.patch


GTK_2_12_DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/platform/2.20/2.20.0/sources/pango-1.18.2.tar.bz2	\
	http://ftp.gnome.org/pub/GNOME/platform/2.20/2.20.0/sources/atk-1.20.0.tar.bz2		\
	http://ftp.gnome.org/pub/GNOME/platform/2.20/2.20.0/sources/gtk+-2.12.0.tar.bz2		\
	http://ftp.gnome.org/pub/GNOME/platform/2.20/2.20.0/sources/libglade-2.6.2.tar.bz2

api: api-2.12

api-2.12:
	PATH=../parser:$$PATH $(RUNTIME) ../parser/gapi-parser.exe gtk-sharp-2.12-sources.xml

get-source-code: get-2.12-sources 

get-2.12-sources:
	for i in $(GTK_2_12_DOWNLOADS); do                  \
		wget $$i --output-document=- | tar -xj ;   \
	done;
	ln -f -s gtkfilechooserprivate.h gtk+-2.12.0/gtk/gtkfilechooserpriv.h
	patch -p0 gtk+-2.12.0/gtk/gtktreemodel.c < gtk_tree_model_signal_fix-2.10.patch
	echo "typedef struct _GtkClipboard GtkClipboard;" >> gtk+-2.12.0/gtk/gtkclipboard.h
	echo "typedef struct _GtkClipboardClass GtkClipboardClass;" >> gtk+-2.12.0/gtk/gtkclipboard.h

