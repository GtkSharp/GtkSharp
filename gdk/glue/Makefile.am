lib_LTLIBRARIES = libgdksharpglue-2.la

libgdksharpglue_2_la_SOURCES =	\
	dragcontext.c		\
	device.c		\
	vmglueheaders.h		\
	windowmanager.c

nodist_libgdksharpglue_2_la_SOURCES = generated.c

# Adding a new glue file?

libgdksharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libgdksharpglue_2_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgdksharpglue.dll: $(libgdksharpglue_2_la_OBJECTS) libgdksharpglue.rc libgdksharpglue.def
	./build-dll libgdksharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
