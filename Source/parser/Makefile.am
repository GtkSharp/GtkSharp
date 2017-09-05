assemblydir = $(prefix)/lib/gapi-3.0
pkgconfigdir = $(libdir)/pkgconfig
assembly_DATA = gapi-parser.exe
pkgconfig_DATA = gapi-3.0.pc
bin_SCRIPTS = gapi3-parser
assembly_SCRIPTS = gapi_pp.pl gapi2xml.pl
CLEANFILES = gapi-parser.exe
DISTCLEANFILES = gapi3-parser gapi-3.0.pc

sources = 				\
	gapi-parser.cs

EXTRA_DIST =					\
	$(sources)				\
	gapi3-parser.in				\
	gapi_pp.pl				\
	gapi2xml.pl				\
	gapi-3.0.pc.in

gapi-parser.exe: $(srcdir)/gapi-parser.cs
	$(CSC) $(CSFLAGS) -out:gapi-parser.exe $(srcdir)/gapi-parser.cs

