API = pango-api.xml
RAW_API = pango-api.raw
METADATA = Pango.metadata
ASSEMBLY_NAME = pango-sharp
references = /r:../glib/glib-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(ASSEMBLY)
gapi_DATA = $(API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

sources = 		\
	Scale.cs

build_sources = $(addprefix $(srcdir)/, $(sources))
dist_sources = $(sources)

customs =			\
	AttrIterator.custom	\
	GlyphItem.custom	\
	Layout.custom

build_customs = $(addprefix $(srcdir)/, $(customs))
dist_customs = $(customs)

EXTRA_DIST =		\
	makefile.win32	\
	$(RAW_API)	\
	$(METADATA)	\
	$(dist_customs)	\
	$(dist_sources)

$(API): $(srcdir)/$(METADATA) $(srcdir)/$(RAW_API)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API)		  \
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

