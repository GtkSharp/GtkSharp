if ENABLE_RSVG
TARGET = $(ASSEMBLY)
APIS = $(API)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = rsvg-sharp.pc
else
TARGET = 
APIS = 
endif

API = rsvg-api.xml
RAW_API = rsvg-api.raw
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml ../gtk/gtk-api.xml $(top_srcdir)/gtk/gtk-symbols.xml ../art/art-api.xml
METADATA = Rsvg.metadata
ASSEMBLY_NAME = rsvg-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll /r:../gtk/gtk-sharp.dll /r:../art/art-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

DISTCLEANFILES = rsvg-sharp.pc

sources =			\
	Tool.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

customs =

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(METADATA)		\
	$(customs)		\
	$(sources)		\
	rsvg-sharp.pc.in

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

