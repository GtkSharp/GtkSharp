lib_LTLIBRARIES = libgtksharpglue.la

BASESOURCES = \
	value.c			\
	fileselection.c		\
	dialog.c		\
	colorseldialog.c        \
	error.c			\
	event.c			\
	slist.c			\
	paned.c			\
	style.c			\
	widget.c			

if ENABLE_GNOME
libgtksharpglue_la_SOURCES = 	\
	$(BASESOURCES) \
	program.c \
	#
else
libgtksharpglue_la_SOURCES = 	\
	$(BASESOURCES) 
endif

INCLUDES = @BASE_DEPENDENCIES_CFLAGS@ -I$(top_srcdir)


libgtksharpglue.dll: $(libgtksharpglue_la_OBJECTS) libgtksharpglue.rc libgtksharpglue.def
	./build-dll libgtksharpglue 0.1

CLEANFILES = lib*.a lib*.dll

