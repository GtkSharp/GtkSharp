lib_LTLIBRARIES = $(TARGET)

if ENABLE_GNOME
TARGET = libgnomesharpglue-2.0.la
else
TARGET = 
endif

libgnomesharpglue_2_0_la_LDFLAGS = -module -avoid-version -no-undefined

libgnomesharpglue_2_0_la_SOURCES = 	\
	canvas.c 		\
	canvasitem.c 		\
	canvaspoints.c 		\
	canvas-proxy.c		\
	canvas-proxy.h 		\
	canvas-proxy-marshal.c 	\
	canvas-proxy-marshal.h 	\
	program.c		\
	#

libgnomesharpglue_2_0_la_LIBADD = $(GNOME_LIBS)

INCLUDES = $(GNOME_CFLAGS) -I$(top_srcdir)

libgnomesharpglue.dll: $(libgnomesharpglue_2_0_la_OBJECTS) libgnomesharpglue.rc libgnomesharpglue.def
	./build-dll libgnomesharpglue-2.0 $(VERSION)

CLEANFILES = lib*.a lib*.dll

