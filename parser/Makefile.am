assemblydir = $(bindir)
pkgconfigdir = $(libdir)/pkgconfig
assembly_DATA = gapi-fixup.exe
pkgconfig_DATA = gapi.pc
bin_SCRIPTS = gapi.pl gapi_pp.pl gapi2xml.pl gapi-fixup gapi-parser
CLEANFILES = gapi-fixup.exe
DISTCLEANFILES = gapi-fixup gapi.pc

sources = 				\
	gapi-fixup.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

EXTRA_DIST =					\
	makefile.win32				\
	$(sources)				\
	gapi-parser				\
	gapi.pl					\
	gapi_pp.pl				\
	gapi2xml.pl				\
	gapi.pc.in

gapi-fixup.exe: $(build_sources)
	$(CSC) /out:gapi-fixup.exe $(build_sources)

INCLUDES = $(GLIB_CFLAGS) $(XML_CFLAGS)

bin_PROGRAMS = gapi_format_xml

gapi_format_xml_SOURCES = formatXml.c

gapi_format_xml_LDADD = $(GLIB_LIBS) $(XML_LIBS)

