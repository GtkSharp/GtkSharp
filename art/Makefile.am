if ENABLE_LIBART
pkg = art
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = art-sharp-2.0.pc
else
pkg = 
endif

SYMBOLS = art-symbols.xml
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml
METADATA = Art.metadata
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll 

sources =

customs = 

add_dist = art-sharp-2.0.pc.in

include ../Makefile.include

