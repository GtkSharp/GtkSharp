if ENABLE_GTKHTML
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
APIS = $(API)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gtkhtml-sharp-2.0.pc
else
TARGET = 
APIS = 
endif
API = gtkhtml-api.xml
RAW_API = gtkhtml-api.raw
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml ../gtk/gtk-api.xml $(top_srcdir)/gtk/gtk-symbols.xml ../art/art-api.xml ../gnome/gnome-api.xml
METADATA = Gtk.metadata
ASSEMBLY_NAME = gtkhtml-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll /r:../gtk/gtk-sharp.dll /r:../art/art-sharp.dll /r:../gnome/gnome-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi-2.0
noinst_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) gtk-sharp.snk

DISTCLEANFILES = gtkhtml-sharp-2.0.pc $(ASSEMBLY).config AssemblyInfo.cs

sources =

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs =			\
	HTML.custom		\
	HTMLStream.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(METADATA)		\
	$(customs)		\
	$(sources)		\
	gtkhtml-sharp-2.0.pc.in

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API)  \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
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

