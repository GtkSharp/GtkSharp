if ENABLE_VTE
pkg = vte
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = vte-sharp-2.0.pc
else
pkg = 
endif
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml ../gtk/gtk-api.xml
METADATA = Vte.metadata
SYMBOLS =
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll

sources = 

customs =

add_dist = vte-sharp-2.0.pc.in

include ../Makefile.include

