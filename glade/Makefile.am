SUBDIRS = glue

if ENABLE_GLADE
pkg = glade
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = glade-sharp-2.0.pc
else
pkg = 
endif

INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml ../gtk/gtk-api.xml
METADATA = Glade.metadata
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll

sources =				\
	HandlerNotFoundExeception.cs	\
	WidgetAttribute.cs

customs =				\
	XML.custom

add_dist = makefile.win32 glade-sharp-2.0.pc.in

include ../Makefile.include

