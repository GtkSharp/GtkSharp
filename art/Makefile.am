if ENABLE_LIBART
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
APIS = $(API) $(ADDITIONAL_API)
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = art-sharp.pc
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
apidir = $(datadir)/gapi
noinst_DATA = $(TARGET)
api_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) gtk-sharp.pub

DISTCLEANFILES = art-sharp.pc $(ASSEMBLY).config AssemblyInfo.cs

sources =

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs
dist_sources = $(sources)

customs = 

build_customs = $(addprefix $(srcdir)/, $(customs))
dist_customs = $(customs)

EXTRA_DIST =			\
	$(RAW_API)		\
	$(ADDITIONAL_API)	\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(METADATA)		\
	$(dist_customs)		\
	$(dist_sources)		\
	art-sharp.pc.in

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) $(srcdir)/$(ADDITIONAL_API) ../generator/gapi_codegen.exe $(build_customs)
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

gtk-sharp.pub: $(top_srcdir)/gtk-sharp.pub
	cp $(top_srcdir)/gtk-sharp.pub .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.pub
	$(CSC) /unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

