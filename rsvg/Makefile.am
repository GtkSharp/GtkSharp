if ENABLE_RSVG
TARGET = $(ASSEMBLY)
APIS = $(API)
else
TARGET = 
APIS = 
endif

API = rsvg-api.xml
RAW_API = rsvg-api.raw
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml ../gtk/gtk-api.xml $(top_srcdir)/gtk/gtk-symbols.xml ../art/art-api.xml
ASSEMBLY_NAME = rsvg-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll /r:../gtk/gtk-sharp.dll /r:../art/art-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

sources =			\
	Tool.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

customs =

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

