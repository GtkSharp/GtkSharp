DOWNLOADS = \
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/pango-1.2.3.tar.gz          	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/atk-1.2.4.tar.gz 		\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/gtk+-2.2.2.tar.gz     		\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/libgnome-2.2.3.tar.gz       	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/libgnomecanvas-2.2.1.tar.gz 	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/libgnomeui-2.2.2.tar.gz     	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/libgnomeprint-2.2.1.3.tar.gz	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/libgnomeprintui-2.2.1.3.tar.gz	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.0/sources/libglade-2.0.0.tar.gz		\
	http://ftp.gnome.org/pub/GNOME/desktop/2.0/2.0.3/sources/libart_lgpl-2.3.10.tar.gz	\
	http://ftp.gnome.org/pub/GNOME/desktop/2.2/2.2.2/sources/librsvg-2.2.5.tar.gz         	\
	http://ftp.gnome.org/pub/GNOME/sources/gstreamer/0.4/gstreamer-0.4.2.tar.gz		\
	http://ftp.gnome.org/pub/GNOME/sources/libgda/1.0/libgda-1.0.0.tar.gz			\
	http://ftp.gnome.org/pub/GNOME/sources/libgnomedb/1.0/libgnomedb-1.0.0.tar.gz		\

GTKHTML_SOURCES = \
        gtkhtml-3.0.8/src/gtkhtml.c \
        gtkhtml-3.0.8/src/gtkhtml.h \
        gtkhtml-3.0.8/src/gtkhtml-types.h \
        gtkhtml-3.0.8/src/gtkhtml-enums.h \
        gtkhtml-3.0.8/src/gtkhtml-stream.c \
        gtkhtml-3.0.8/src/gtkhtml-stream.h

all:
	PERLLIB=../parser PATH=../parser:$$PATH ../parser/gapi.pl gtk-sharp.sources ../api

get-gtkhtml-code:
	wget http://ftp.gnome.org/pub/GNOME/sources/gtkhtml/3.0/gtkhtml-3.0.8.tar.gz --output-document=- | tar -xz $(GTKHTML_SOURCES)
	patch -p0 < gtkhtml-font-style-enum.patch

get-source-code: get-gtkhtml-code
	for i in $(DOWNLOADS); do                          \
		wget $$i --output-document=- | tar -xz ;   \
	done;

	ln -f -s ../gdk/gdkpixbuf-drawable.c gtk+-2.2.2/gdk-pixbuf/gdkpixbuf-drawable.c
	ln -f -s ../gdk/gdkpixbuf.h          gtk+-2.2.2/gdk-pixbuf/gdkpixbuf.h
	ln -f -s ../gdk/gdkpixbuf-render.c   gtk+-2.2.2/gdk-pixbuf/gdkpixbuf-render.c

distclean:
	for i in `ls`; do                     \
	    if test -d $$i; then              \
	        if test 'CVS' != $$i; then    \
	            rm -Rf $$i;               \
	        fi;                           \
	    fi;                               \
	done;
