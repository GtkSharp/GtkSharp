if ENABLE_GTKHTML
pkg = gtkhtml
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gtkhtml-sharp-2.0.pc
else
pkg = 
endif
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml ../gtk/gtk-api.xml ../art/art-api.xml ../gnome/gnome-api.xml
METADATA = Gtk.metadata
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll ../art/art-sharp.dll ../gnome/gnome-sharp.dll

sources =

customs =			\
	HTML.custom		\
	HTMLStream.custom

add_dist = gtkhtml-sharp-2.0.pc.in

include ../Makefile.include

