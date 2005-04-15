assemblydir = $(libdir)/gtk-sharp-2.0
pkgconfigdir = $(libdir)/pkgconfig
assembly_DATA = gapi-fixup.exe gapi-parser.exe
pkgconfig_DATA = gapi-2.0.pc
bin_SCRIPTS = gapi2-fixup gapi2-parser
assembly_SCRIPTS = gapi_pp.pl gapi2xml.pl
CLEANFILES = gapi-fixup.exe
DISTCLEANFILES = gapi2-fixup gapi2-parser gapi-2.0.pc

sources = 				\
	gapi-fixup.cs			\
	gapi-parser.cs

EXTRA_DIST =					\
	makefile.win32				\
	$(sources)				\
	gapi2-parser.in				\
	gapi_pp.pl				\
	gapi2xml.pl				\
	gapi-2.0.pc.in

gapi-fixup.exe: $(srcdir)/gapi-fixup.cs
	$(CSC) /out:gapi-fixup.exe $(srcdir)/gapi-fixup.cs

gapi-parser.exe: $(srcdir)/gapi-parser.cs
	$(CSC) /out:gapi-parser.exe $(srcdir)/gapi-parser.cs

INCLUDES = $(GLIB_CFLAGS) $(XML_CFLAGS)

assembly_PROGRAMS = gapi_format_xml

gapi_format_xml_SOURCES = formatXml.c

gapi_format_xml_LDADD = $(GLIB_LIBS) $(XML_LIBS)

