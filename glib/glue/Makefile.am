lib_LTLIBRARIES = libglibsharpglue.la

libglibsharpglue_la_LDFLAGS = -module -avoid-version

libglibsharpglue_la_SOURCES = 	\
	error.c			\
	list.c			\
	object.c		\
	slist.c			\
	time_t.c		\
	type.c			\
	unichar.c		\
	value.c			\
	valuearray.c

# Adding a new glue file?
# Please remember to update makefile.win32

libglibsharpglue_la_LIBADD = $(BASE_DEPENDENCIES_LIBS)

INCLUDES = $(BASE_DEPENDENCIES_CFLAGS) -I$(top_srcdir)

libgtksharpglue.dll: $(libgtksharpglue_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
