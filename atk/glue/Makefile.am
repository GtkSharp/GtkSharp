lib_LTLIBRARIES = libatksharpglue.la

libatksharpglue_la_LDFLAGS = -module -avoid-version -no-undefined

libatksharpglue_la_SOURCES = 	\
	generated.c		\
	vmglueheaders.h

# Adding a new glue file?
# Please remember to update makefile.win32

libatksharpglue_la_LIBADD = $(BASE_DEPENDENCIES_LIBS)

INCLUDES = $(BASE_DEPENDENCIES_CFLAGS) -I$(top_srcdir)

libatksharpglue.dll: $(libatksharpglue_la_OBJECTS) libatksharpglue.rc libatksharpglue.def
	./build-dll libatksharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
