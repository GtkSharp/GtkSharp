lib_LTLIBRARIES = libgtksharpglue.la

BASESOURCES = \
	adjustment.c		\
	value.c			\
	fileselection.c		\
	dialog.c		\
	colorseldialog.c        \
	error.c			\
	event.c			\
	slist.c			\
	object.c		\
	paned.c			\
	style.c			\
	type.c			\
	widget.c		\
	list.c			\
	#

GNOMESOURCES = \
	canvasitem.c \
	canvaspoints.c \
	canvas-proxy.c	\
	canvas-proxy.h \
	canvas-proxy-marshal.c \
	canvas-proxy-marshal.h \
	program.c	\
	#

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

INCLUDES = @BASE_DEPENDENCIES_CFLAGS@ -I$(top_srcdir)


libgtksharpglue.dll: $(libgtksharpglue_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue 0.1

CLEANFILES = lib*.a lib*.dll

