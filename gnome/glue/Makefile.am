lib_LTLIBRARIES = $(TARGET)

if ENABLE_GNOME
TARGET = libgnomesharpglue-2.la
else
TARGET = 
endif

libgnomesharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libgnomesharpglue_2_la_SOURCES = 	\
	canvas.c 		\
	canvasitem.c 		\
	canvaspoints.c 		\
	panelapplet.c		\
	program.c		\
	#

libgnomesharpglue_2_la_LIBADD = $(GNOME_LIBS)

INCLUDES = $(GNOME_CFLAGS) -I$(top_srcdir)

libgnomesharpglue.dll: $(libgnomesharpglue_2_la_OBJECTS) libgnomesharpglue.rc libgnomesharpglue.def
	./build-dll libgnomesharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

