lib_LTLIBRARIES = $(TARGET)

if ENABLE_GLADE
TARGET = libgladesharpglue.la
else
TARGET = 
endif

libgladesharpglue_la_LDFLAGS = -module -avoid-version -no-undefined

libgladesharpglue_la_SOURCES = 	\
	gladexml.c

libgladesharpglue_la_LIBADD = $(GLADE_LIBS)

INCLUDES = $(GLADE_CFLAGS) -I$(top_srcdir)

libgladesharpglue.dll: $(libgladesharpglue_la_OBJECTS) libgladesharpglue.rc libgladesharpglue.def
	./build-dll libgladesharpglue $(VERSION)

CLEANFILES = lib*.a lib*.dll

EXTRA_DIST = makefile.win32 win32dll.c
