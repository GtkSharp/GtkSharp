if ENABLE_GDA
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
APIS = $(API)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gda-sharp-2.0.pc
else
TARGET = 
APIS = 
endif

API = gda-api.xml
RAW_API = gda-api.raw
METADATA = Gda.metadata
ASSEMBLY_NAME = gda-sharp
references = /r:../glib/glib-sharp.dll 

ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi-2.0
noinst_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) gtk-sharp.snk

DISTCLEANFILES = gda-sharp-2.0.pc $(ASSEMBLY).config AssemblyInfo.cs

sources = 				\
	Application.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs = 

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(METADATA)		\
	$(customs)		\
	$(sources)		\
	gda-sharp-2.0.pc.in

$(API): $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) \
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.snk
	$(CSC) /nowarn:0612 /unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) $(GENERATED_SOURCES)

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

