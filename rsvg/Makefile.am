if ENABLE_RSVG
pkg = rsvg
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = rsvg-sharp-2.0.pc
else
pkg = 
endif

INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml ../gtk/gtk-api.xml ../art/art-api.xml
METADATA = Rsvg.metadata
SYMBOLS =
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll ../art/art-sharp.dll

sources =			\
	Tool.cs

customs = Pixbuf.custom

add_dist = rsvg-sharp-2.0.pc.in

include ../Makefile.include

