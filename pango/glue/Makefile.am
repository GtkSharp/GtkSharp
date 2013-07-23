lib_LTLIBRARIES = libpangosharpglue-3.la

libpangosharpglue_3_la_SOURCES =

nodist_libpangosharpglue_3_la_SOURCES = generated.c

# Adding a new glue file?

libpangosharpglue_3_la_LDFLAGS = -module -avoid-version -no-undefined

libpangosharpglue_3_la_LIBADD = $(PANGO_LIBS)

AM_CPPFLAGS = $(PANGO_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libpangosharpglue.dll: $(libpangosharpglue_3_la_OBJECTS) libpangosharpglue.rc libpangosharpglue.def
	./build-dll libpangosharpglue-3 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
