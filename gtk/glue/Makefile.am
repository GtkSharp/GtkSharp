lib_LTLIBRARIES = libgtksharpglue.la

libgtksharpglue_la_SOURCES =	\
	adjustment.c		\
	button.c		\
	cellrenderer.c		\
	clipboard.c		\
	colorseldialog.c        \
	combo.c			\
	container.c		\
	dialog.c		\
	fileselection.c		\
	layout.c		\
	nodestore.c		\
	object.c		\
	paned.c			\
	selectiondata.c		\
	style.c			\
	vmglueheaders.h		\
	widget.c

# Adding a new glue file?
# Please remember to update makefile.win32

libgtksharpglue_la_LDFLAGS = -module -avoid-version -no-undefined

libgtksharpglue_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS) -I$(top_srcdir)

libgtksharpglue.dll: $(libgtksharpglue_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
