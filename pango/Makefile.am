SUBDIRS = . glue

API = pango-api.xml
RAW_API = pango-api.raw
METADATA = Pango.metadata
ASSEMBLY_NAME = pango-sharp
references = /r:../glib/glib-sharp.dll

TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
gapi_DATA = $(API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) gtk-sharp.snk
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs

sources = 		\
	Scale.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs
dist_sources = $(sources)

customs =			\
	AttrIterator.custom	\
	GlyphItem.custom	\
	Layout.custom		\
	LayoutLine.custom

build_customs = $(addprefix $(srcdir)/, $(customs))
dist_customs = $(customs)

EXTRA_DIST =			\
	makefile.win32		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(RAW_API)		\
	$(METADATA)		\
	$(dist_customs)		\
	$(dist_sources)

$(API): $(srcdir)/$(METADATA) $(srcdir)/$(RAW_API)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API)		  \
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.snk
	$(CSC) /unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;	\
	fi

