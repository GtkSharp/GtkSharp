lib_LTLIBRARIES = libglibsharpglue-3.la

libglibsharpglue_3_la_LDFLAGS = -module -avoid-version -no-undefined

libglibsharpglue_3_la_SOURCES = 	\
	thread.c

# Adding a new glue file?

libglibsharpglue_3_la_LIBADD = $(GLIB_LIBS)

INCLUDES = $(GLIB_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libglibsharpglue.dll: $(libglibsharpglue_3_la_OBJECTS) libglibsharpglue.rc libglibsharpglue.def
	./build-dll libglibsharpglue-3 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
