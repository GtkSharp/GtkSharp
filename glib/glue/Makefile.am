lib_LTLIBRARIES = libglibsharpglue-2.0.la

libglibsharpglue_2_0_la_LDFLAGS = -module -avoid-version -no-undefined

libglibsharpglue_2_0_la_SOURCES = 	\
	error.c			\
	list.c			\
	object.c		\
	slist.c			\
	type.c			\
	unichar.c		\
	value.c			\
	valuearray.c

# Adding a new glue file?
# Please remember to update makefile.win32

libglibsharpglue_2_0_la_LIBADD = $(GLIB_LIBS)

INCLUDES = $(GLIB_CFLAGS) -I$(top_srcdir)

libgtksharpglue.dll: $(libgtksharpglue_2_0_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue-2.0 $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
