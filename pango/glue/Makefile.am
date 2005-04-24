lib_LTLIBRARIES = libpangosharpglue-2.la

libpangosharpglue_2_la_SOURCES =	\
	attribute.c			\
	layoutline.c

# Adding a new glue file?
# Please remember to update makefile.win32

libpangosharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libpangosharpglue_2_la_LIBADD = $(PANGO_LIBS)

INCLUDES = $(PANGO_CFLAGS) -I$(top_srcdir)

libpangosharpglue.dll: $(libpangosharpglue_2_la_OBJECTS) libpangosharpglue.rc libpangosharpglue.def
	./build-dll libpangosharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
