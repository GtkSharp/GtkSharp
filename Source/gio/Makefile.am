SUBDIRS = . glue

pkg = gio
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gio-sharp-3.0.pc
INCLUDE_API = $(top_srcdir)/glib/glib-api.xml
METADATA = Gio.metadata
SYMBOLS = 
references = ../glib/glib-sharp.dll
glue_includes = gio/gio.h

POLICY_VERSIONS=

sources =			\
	Application.cs		\
	AppInfoAdapter.cs	\
	FileAdapter.cs		\
	FileEnumerator.cs	\
	FileFactory.cs		\
	GioGlobal.cs		\
	GioStream.cs		\
	IFile.cs

add_dist = gio-sharp-3.0.pc.in

include ../Makefile.include

