if ENABLE_GDA
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
APIS = $(API)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gda-sharp.pc
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
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

DISTCLEANFILES = gda-sharp.pc $(ASSEMBLY).config

sources = 				\
	Application.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

customs = 

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(ASSEMBLY).config.in	\
	$(METADATA)		\
	$(customs)		\
	$(sources)		\
	gda-sharp.pc.in

$(API): $(srcdir)/$(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) \
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

