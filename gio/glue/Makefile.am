lib_LTLIBRARIES = libgiosharpglue-2.la

libgiosharpglue_2_la_SOURCES =

nodist_libgiosharpglue_2_la_SOURCES = generated.c

libgiosharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libgiosharpglue_2_la_LIBADD = $(GIO_LIBS)

INCLUDES = $(GIO_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgiosharpglue.dll: $(libgiosharpglue_2_la_OBJECTS) libgiosharpglue.rc libgiosharpglue.def
	./build-dll libgiosharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
