lib_LTLIBRARIES = libgtksharpglue.la

BASESOURCES = 			\
	adjustment.c		\
	button.c		\
	clipboard.c		\
	colorseldialog.c        \
	combo.c			\
	dialog.c		\
	error.c			\
	event.c			\
	fileselection.c		\
	layout.c		\
	list.c			\
	object.c		\
	paned.c			\
	selection.c		\
	slist.c			\
	style.c			\
	type.c			\
	value.c			\
	widget.c

# Adding a new glue file?
# Please remember to update makefile.win32

GNOMESOURCES = \
	canvasitem.c \
	canvaspoints.c \
	canvas-proxy.c	\
	canvas-proxy.h \
	canvas-proxy-marshal.c \
	canvas-proxy-marshal.h \
	program.c	\
	#

libgtksharpglue_la_LDFLAGS = -module -avoid-version

# fixme: Makefile.am = THE SUCK
if ENABLE_GNOME

if ENABLE_GLADE
libgtksharpglue_la_SOURCES = 	\
	$(BASESOURCES) \
	$(GNOMESOURCES) \
	gladexml.c	\
	#
else
libgtksharpglue_la_SOURCES = 	\
	$(BASESOURCES) \
	$(GNOMESOURCES) \
	#
endif

else

if ENABLE_GLADE
libgtksharpglue_la_SOURCES = 	\
	$(BASESOURCES) \
	gladexml.c	\
	#
else
libgtksharpglue_la_SOURCES = 	\
	$(BASESOURCES) 
endif

endif

libgtksharpglue_la_LIBADD = $(BASE_DEPENDENCIES_LIBS)

INCLUDES = @BASE_DEPENDENCIES_CFLAGS@ -I$(top_srcdir)


libgtksharpglue.dll: $(libgtksharpglue_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue 0.1

CLEANFILES = lib*.a lib*.dll

