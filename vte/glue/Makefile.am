lib_LTLIBRARIES = $(TARGET)

if ENABLE_VTE
TARGET = libvtesharpglue-2.la
else
TARGET = 
endif

libvtesharpglue_2_la_SOURCES = # empty

nodist_libvtesharpglue_2_la_SOURCES = generated.c

libvtesharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libvtesharpglue_2_la_LIBADD = $(VTE_LIBS)

INCLUDES = $(GTK_CFLAGS) $(VTE_CFLAGS) -I$(top_srcdir)

libvtesharpglue.dll: $(libvtesharpglue_2_la_OBJECTS) libvtesharpglue.rc libvtesharpglue.def
	./build-dll libvtesharpglue-2 $(VERSION)

CLEANFILES = lib*.a lib*.dll

