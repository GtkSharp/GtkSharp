DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/atk-1.0.2.tar.gz            \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/pango-1.0.3.tar.gz          \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/gtk+-2.0.5.tar.gz           \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/libgnome-2.0.1.tar.gz       \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/libgnomecanvas-2.0.1.tar.gz \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/libgnomeui-2.0.1.tar.gz     \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/libglade-2.0.0.tar.gz       \
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.3/sources/libart_lgpl-2.3.10.tar.gz   \
	ftp://ftp.gnome-db.org/pub/gnome-db/sources/v0.10.0/libgda-0.10.0.tar.gz             \
	ftp://ftp.gnome-db.org/pub/gnome-db/sources/v0.10.0/libgnomedb-0.10.0.tar.gz         \
	http://unc.dl.sourceforge.net/sourceforge/gstreamer/gstreamer-0.4.2.tar.gz           \
	http://ftp.gnome.org/pub/gnome/sources/librsvg/2.0/librsvg-2.0.1.tar.gz              \


all:
	PERLLIB=../parser PATH=../parser:$$PATH ../parser/gapi.pl gtk-sharp.sources ../api

get-source-code:
	for i in $(DOWNLOADS); do                          \
		wget $$i --output-document=- | tar -xz ;   \
	done;
	export CVS_PASSWORD=""
	cvs -z3 -d:pserver:anonymous@anoncvs.gnome.org:/cvs/gnome co gtkhtml/src/gtkhtml.c
	cvs -z3 -d:pserver:anonymous@anoncvs.gnome.org:/cvs/gnome co gtkhtml/src/gtkhtml.h
	cvs -z3 -d:pserver:anonymous@anoncvs.gnome.org:/cvs/gnome co gtkhtml/src/gtkhtml-types.h
	cvs -z3 -d:pserver:anonymous@anoncvs.gnome.org:/cvs/gnome co gtkhtml/src/gtkhtml-enums.h
	cvs -z3 -d:pserver:anonymous@anoncvs.gnome.org:/cvs/gnome co gtkhtml/src/gtkhtml-stream.c
	cvs -z3 -d:pserver:anonymous@anoncvs.gnome.org:/cvs/gnome co gtkhtml/src/gtkhtml-stream.h
	ln -f -s ../gdk/gdkpixbuf-drawable.c gtk+-2.0.5/gdk-pixbuf/gdkpixbuf-drawable.c
	ln -f -s ../gdk/gdkpixbuf.h          gtk+-2.0.5/gdk-pixbuf/gdkpixbuf.h
	ln -f -s ../gdk/gdkpixbuf-render.c   gtk+-2.0.5/gdk-pixbuf/gdkpixbuf-render.c
 
 
