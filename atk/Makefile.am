#SUBDIRS = . glue

API = atk-api.xml
RAW_API = atk-api.raw
METADATA = Atk.metadata
ASSEMBLY_NAME = atk-sharp
references = /r:../glib/glib-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(ASSEMBLY) $(ASSEMBLY).config
gapi_DATA = $(API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) glue/generated.c
DISTCLEANFILES = $(ASSEMBLY).config

sources = 

build_sources = $(addprefix $(srcdir)/, $(sources))

customs = 

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	makefile.win32		\
	$(RAW_API)		\
	$(ASSEMBLY).config.in	\
	$(METADATA)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API)			\
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME)	\
	--gluelib-name=atksharpglue --glue-filename=glue/generated.c			\
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

