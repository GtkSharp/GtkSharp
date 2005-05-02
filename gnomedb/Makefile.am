if ENABLE_GNOMEDB
pkg = gnomedb
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gnomedb-sharp-2.0.pc
else
pkg = 
APIS = 
endif
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml ../gtk/gtk-api.xml ../art/art-api.xml ../gnome/gnome-api.xml ../gda/gda-api.xml
METADATA = GnomeDb.metadata
SYMBOLS =
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll ../art/art-sharp.dll ../gnome/gnome-sharp.dll ../gda/gda-sharp.dll

sources =				\
	Application.cs

customs =				\
	Editor.custom

add_dist = gnomedb-sharp-2.0.pc.in

include ../Makefile.include

