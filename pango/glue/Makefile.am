lib_LTLIBRARIES = libpangosharpglue.la

libpangosharpglue_la_SOURCES =	\
	layoutline.c

# Adding a new glue file?
# Please remember to update makefile.win32

libpangosharpglue_la_LDFLAGS = -module -avoid-version

libpangosharpglue_la_LIBADD = $(PANGO_LIBS)

INCLUDES = $(PANGO_CFLAGS) -I$(top_srcdir)

libpangosharpglue.dll: $(libpangosharpglue_la_OBJECTS) libpangosharpglue.rc libpangosharpglue.def
	./build-dll libpangosharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
