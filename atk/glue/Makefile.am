lib_LTLIBRARIES = libatksharpglue-2.la

libatksharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libatksharpglue_2_la_SOURCES = 	\
	object.c		\
	util.c			\
	vmglueheaders.h

# Adding a new glue file?
# Please remember to update makefile.win32

libatksharpglue_2_la_LIBADD = $(ATK_LIBS)

INCLUDES = $(ATK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libatksharpglue.dll: $(libatksharpglue_2_la_OBJECTS) libatksharpglue.rc libatksharpglue.def
	./build-dll libatksharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
