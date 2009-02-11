SUBDIRS = . glue

pkg = gio
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gio-sharp-2.0.pc
METADATA = Gio.metadata
SYMBOLS = 
references = ../glib/glib-sharp.dll
glue_includes = gio/gio.h

POLICY_VERSIONS=

sources =			\
	FileFactory.cs		\
	GioStream.cs

customs =			\
	AppInfoAdapter.custom	\
	File.custom		\
	FileAdapter.custom	\
	FileEnumerator.custom

add_dist = gio-sharp-2.0.pc.in

include ../Makefile.include

