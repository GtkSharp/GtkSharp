lib_LTLIBRARIES = libglibsharpglue-2.la

libglibsharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libglibsharpglue_2_la_SOURCES = 	\
	closure.c		\
	error.c			\
	list.c			\
	object.c		\
	signal.c		\
	slist.c			\
	type.c			\
	unichar.c		\
	value.c			\
	valuearray.c	\
	thread.c

# Adding a new glue file?

libglibsharpglue_2_la_LIBADD = $(GLIB_LIBS)

INCLUDES = $(GLIB_CFLAGS) $(GTK_SHARP_VERSION_CFLAGS) -I$(top_srcdir)

libgtksharpglue.dll: $(libgtksharpglue_2_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = win32dll.c
