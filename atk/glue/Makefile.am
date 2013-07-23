lib_LTLIBRARIES = libatksharpglue-3.la

libatksharpglue_3_la_LDFLAGS = -module -avoid-version -no-undefined

libatksharpglue_3_la_SOURCES = 	\
	misc.c			\
	util.c			\
	vmglueheaders.h

nodist_libatksharpglue_3_la_SOURCES = generated.c

# Adding a new glue file?

libatksharpglue_3_la_LIBADD = $(ATK_LIBS)

AM_CPPFLAGS = $(ATK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libatksharpglue.dll: $(libatksharpglue_3_la_OBJECTS) libatksharpglue.rc libatksharpglue.def
	./build-dll libatksharpglue-3 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
