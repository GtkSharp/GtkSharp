lib_LTLIBRARIES = $(TARGET)

if ENABLE_GNOME
TARGET = libgnomesharpglue.la
else
TARGET = 
endif

libgnomesharpglue_la_LDFLAGS = -module -avoid-version

libgnomesharpglue_la_SOURCES = 	\
	canvasitem.c 		\
	canvaspoints.c 		\
	canvas-proxy.c		\
	canvas-proxy.h 		\
	canvas-proxy-marshal.c 	\
	canvas-proxy-marshal.h 	\
	program.c		\
	#

libgnomesharpglue_la_LIBADD = $(BASE_DEPENDENCIES_LIBS)

INCLUDES = $(BASE_DEPENDENCIES_CFLAGS) -I$(top_srcdir)

libgnomesharpglue.dll: $(libgnomesharpglue_la_OBJECTS) libgnomesharpglue.rc libgnomesharpglue.def
	./build-dll libgnomesharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

