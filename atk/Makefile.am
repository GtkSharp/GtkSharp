#SUBDIRS = . glue

API = atk-api.xml
RAW_API = atk-api.raw
METADATA = Atk.metadata
ASSEMBLY_NAME = atk-sharp
references = /r:../glib/glib-sharp.dll

TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi-2.0
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
gapi_DATA = $(API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) glue/generated.c gtk-sharp.snk
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs

sources = 

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs = 

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	makefile.win32		\
	$(RAW_API)		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(METADATA)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API)			\
	--outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME)	\
	--gluelib-name=atksharpglue --glue-filename=glue/generated.c			\
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

