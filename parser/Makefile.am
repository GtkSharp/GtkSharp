assemblydir = $(prefix)/lib/gapi-3.0
pkgconfigdir = $(libdir)/pkgconfig
assembly_DATA = gapi-fixup.exe gapi-parser.exe
pkgconfig_DATA = gapi-3.0.pc
bin_SCRIPTS = gapi3-fixup gapi3-parser
assembly_SCRIPTS = gapi_pp.pl gapi2xml.pl
CLEANFILES = gapi-fixup.exe gapi-parser.exe
DISTCLEANFILES = gapi3-fixup gapi3-parser gapi-3.0.pc

sources = 				\
	gapi-fixup.cs			\
	gapi-parser.cs

EXTRA_DIST =					\
	$(sources)				\
	gapi3-parser.in				\
	gapi_pp.pl				\
	gapi2xml.pl				\
	gapi-3.0.pc.in

gapi-fixup.exe: $(srcdir)/gapi-fixup.cs
	$(CSC) $(CSFLAGS) -out:gapi-fixup.exe $(srcdir)/gapi-fixup.cs

gapi-parser.exe: $(srcdir)/gapi-parser.cs
	$(CSC) $(CSFLAGS) -out:gapi-parser.exe $(srcdir)/gapi-parser.cs

