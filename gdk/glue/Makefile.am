lib_LTLIBRARIES = libgdksharpglue.la

libgdksharpglue_la_SOURCES =	\
	dragcontext.c		\
	event.c			\
	selection.c		\
	vmglueheaders.h		\
	windowmanager.c

# Adding a new glue file?
# Please remember to update makefile.win32

libgdksharpglue_la_LDFLAGS = -module -avoid-version -no-undefined

libgdksharpglue_la_LIBADD = $(GTK_LIBS)

INCLUDES = $(GTK_CFLAGS) -I$(top_srcdir)

libgdksharpglue.dll: $(libgdksharpglue_la_OBJECTS) libgdksharpglue.rc libgdksharpglue.def
	./build-dll libgdksharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
