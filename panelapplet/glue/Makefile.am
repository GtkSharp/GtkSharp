lib_LTLIBRARIES = $(TARGET)

if ENABLE_PANEL
TARGET = libpanelappletsharpglue-2.la
else
TARGET = 
endif

libpanelappletsharpglue_2_la_SOURCES =	\
	panelapplet.c

libpanelappletsharpglue_2_la_LDFLAGS = -module -avoid-version -no-undefined

libpanelappletsharpglue_2_la_LIBADD = $(LIBPANEL_LIBS)

INCLUDES = $(LIBPANEL_CFLAGS) -I$(top_srcdir)

CLEANFILES = lib*.a
