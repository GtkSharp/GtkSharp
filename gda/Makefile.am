if ENABLE_GDA
pkg = gda
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gda-sharp-2.0.pc
else
pkg = 
endif

METADATA = Gda.metadata
SYMBOLS =
references = ../glib/glib-sharp.dll 

sources = 				\
	Application.cs

customs = 				\
	XmlConnection.custom

add_dist = gda-sharp-2.0.pc.in

include ../Makefile.include

