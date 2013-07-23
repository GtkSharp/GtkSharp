lib_LTLIBRARIES = libgiosharpglue-3.la

libgiosharpglue_3_la_SOURCES =

nodist_libgiosharpglue_3_la_SOURCES = generated.c

libgiosharpglue_3_la_LDFLAGS = -module -avoid-version -no-undefined

libgiosharpglue_3_la_LIBADD = $(GIO_LIBS)

AM_CPPFLAGS = $(GIO_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgiosharpglue.dll: $(libgiosharpglue_3_la_OBJECTS) libgiosharpglue.rc libgiosharpglue.def
	./build-dll libgiosharpglue-3 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
