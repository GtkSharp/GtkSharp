lib_LTLIBRARIES = libgtksharpglue.la

INCLUDES = $(GTK_CFLAGS) -I$(top_srcdir)

libgtksharpglue_la_SOURCES = 	\
	value.c			\
	textiter.c		\
	fileselection.c		\
	error.c			\
	#

libgtksharpglue.dll: $(libgtksharpglue_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue 0.1

CLEANFILES = lib*.a lib*.dll

