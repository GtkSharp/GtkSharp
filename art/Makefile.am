if ENABLE_LIBART
TARGET = $(ASSEMBLY)
APIS = $(API) $(ADDITIONAL_API)
else
TARGET = 
APIS = 
endif
API = art-api.xml
RAW_API = art-api.raw
ADDITIONAL_API = art-symbols.xml
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml 
METADATA = Art.metadata
ASSEMBLY_NAME = art-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll 

ASSEMBLY = $(ASSEMBLY_NAME).dll
assemblydir = $(libdir)
apidir = $(datadir)/gapi
assembly_DATA = $(TARGET)
api_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

sources =

build_sources = $(addprefix $(srcdir)/, $(sources))
dist_sources = $(sources)

customs = 

build_customs = $(addprefix $(srcdir)/, $(customs))
dist_customs = $(customs)

EXTRA_DIST =			\
	$(RAW_API)		\
	$(ADDITIONAL_API)	\
	$(METADATA)		\
	$(dist_customs)		\
	$(dist_sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) $(srcdir)/$(ADDITIONAL_API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

