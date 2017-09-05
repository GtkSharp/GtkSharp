lib_LTLIBRARIES = libgtksharpglue-3.la

libgtksharpglue_3_la_SOURCES =	\
	cellrenderer.c		\
	container.c		\
	style.c			\
	vmglueheaders.h		\
	widget.c

nodist_libgtksharpglue_3_la_SOURCES = generated.c

# Adding a new glue file?

libgtksharpglue_3_la_LDFLAGS = -module -avoid-version -no-undefined

libgtksharpglue_3_la_LIBADD = $(GTK_LIBS)

AM_CPPFLAGS = $(GTK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgtksharpglue.dll: $(libgtksharpglue_3_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue-3 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
