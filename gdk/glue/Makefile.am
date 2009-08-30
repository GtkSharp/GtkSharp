lib_LTLIBRARIES = libgdksharpglue-3.la

libgdksharpglue_3_la_SOURCES =	\
	dragcontext.c		\
	device.c		\
	vmglueheaders.h

nodist_libgdksharpglue_3_la_SOURCES = generated.c

# Adding a new glue file?

libgdksharpglue_3_la_LDFLAGS = -module -avoid-version -no-undefined

libgdksharpglue_3_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgdksharpglue.dll: $(libgdksharpglue_3_la_OBJECTS) libgdksharpglue.rc libgdksharpglue.def
	./build-dll libgdksharpglue-3 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
