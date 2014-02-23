if ENABLE_DOTNET
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-dotnet-3.0.pc
else
TARGET =
endif

SNK = $(top_srcdir)/gtk-sharp.snk
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gtk-dotnet
ASSEMBLY_NAME_VERSION = $(ASSEMBLY_NAME),Version=$(API_VERSION)
noinst_DATA = $(ASSEMBLY) $(POLICY_ASSEMBLIES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)
DISTCLEANFILES = $(ASSEMBLY).config

POLICY_ASSEMBLIES = $(addsuffix .$(ASSEMBLY), $(addprefix policy., $(POLICY_VERSIONS)))
POLICY_CONFIGS = $(addsuffix .config, $(addprefix policy., $(POLICY_VERSIONS)))

references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/gio/gio-sharp.dll $(top_builddir)/cairo/cairo-sharp.dll $(top_builddir)/pango/pango-sharp.dll $(top_builddir)/gdk/gdk-sharp.dll $(top_builddir)/gtk/gtk-sharp.dll
build_references = $(addprefix -r:, $(references)) -r:System.Drawing.dll

sources =		 			\
	Graphics.cs				\
	StyleContextExtensions.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

EXTRA_DIST =				\
	$(sources)			\
	$(ASSEMBLY).config.in		\
	gtk-dotnet-3.0.pc.in

$(ASSEMBLY): $(build_sources) $(references) $(SNK)
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -keyfile:$(SNK) -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(build_references) $(build_sources) $(top_builddir)$(ASSEMBLYINFO)

policy.%.config: $(top_builddir)/policy.config
	sed -e "s/@ASSEMBLY_NAME@/$(ASSEMBLY_NAME)/" -e "s/@POLICY@/$*/" $(top_builddir)/policy.config > $@

$(POLICY_ASSEMBLIES) : policy.%.$(ASSEMBLY): policy.%.config $(SNK)
	$(AL) -link:policy.$*.config -out:$@ -keyfile:$(SNK)

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -i $(ASSEMBLY) -f $(GACUTIL_FLAGS)";				\
	  "$(GACUTIL)" -i $(ASSEMBLY) -f $(GACUTIL_FLAGS) || exit 1;			\
	  if test -n '$(POLICY_VERSIONS)'; then						\
	    for i in $(POLICY_VERSIONS); do						\
	      echo "$(GACUTIL) -i policy.$$i.$(ASSEMBLY) -f $(GACUTIL_FLAGS)";		\
	      "$(GACUTIL)" -i policy.$$i.$(ASSEMBLY) -f $(GACUTIL_FLAGS) || exit 1;	\
	    done									\
	  fi										\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) -u $(ASSEMBLY_NAME_VERSION) $(GACUTIL_FLAGS)";			\
	  "$(GACUTIL)" -u $(ASSEMBLY_NAME_VERSION) $(GACUTIL_FLAGS) || exit 1;			\
	  if test -n '$(POLICY_VERSIONS)'; then						\
	    for i in $(POLICY_VERSIONS); do						\
	      echo "$(GACUTIL) -u policy.$$i.$(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";	\
	      "$(GACUTIL)" -u policy.$$i.$(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;	\
	    done									\
	  fi										\
	fi

