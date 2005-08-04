lib_LTLIBRARIES = libgtksharpglue-2.la

libgtksharpglue_2_la_SOURCES =	\
	adjustment.c		\
	cellrenderer.c		\
	clipboard.c		\
	colorseldialog.c	\
	container.c		\
	nodestore.c		\
	object.c		\
	selectiondata.c		\
	style.c			\
	targetlist.c		\
	vmglueheaders.h		\
	widget.c

nodist_libgtksharpglue_2_la_SOURCES = generated.c

# Adding a new glue file?
# Please remember to update makefile.win32

libgtksharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libgtksharpglue_2_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgtksharpglue.dll: $(libgtksharpglue_2_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
