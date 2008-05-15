if ENABLE_DOTNET
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
pkgconfigdir=$(libdir)/pkgconfig
pkgconfig_DATA=gtk-dotnet-2.0.pc
else
TARGET =
endif

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gtk-dotnet
noinst_DATA = $(ASSEMBLY) $(POLICY_ASSEMBLIES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb gtk-sharp.snk AssemblyInfo.cs $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)
DISTCLEANFILES = $(ASSEMBLY).config

POLICY_ASSEMBLIES = $(addsuffix .$(ASSEMBLY), $(addprefix policy., $(POLICY_VERSIONS)))
POLICY_CONFIGS = $(addsuffix .config, $(addprefix policy., $(POLICY_VERSIONS)))

references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../gdk/gdk-sharp.dll
build_references = $(addprefix -r:, $(references)) -r:System.Drawing.dll

sources =		 			\
	Graphics.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

EXTRA_DIST =				\
	$(sources)			\
	$(ASSEMBLY).config.in		\
	gtk-dotnet-2.0.pc.in

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

AssemblyInfo.cs: $(top_builddir)/AssemblyInfo.cs
	cp $(top_builddir)/AssemblyInfo.cs .

if PLATFORM_WIN32
GAPI_CDECL_INSERT=$(top_srcdir)/gapi-cdecl-insert --keyfile=gtk-sharp.snk $(ASSEMBLY)
else
GAPI_CDECL_INSERT=
endif

$(ASSEMBLY): $(build_sources) $(references) gtk-sharp.snk AssemblyInfo.cs
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) -nowarn:0169,0612,0618 -out:$(ASSEMBLY) -target:library $(build_references) $(build_sources)
	$(GAPI_CDECL_INSERT)

$(POLICY_ASSEMBLIES): $(top_builddir)/policy.config gtk-sharp.snk
	@for i in $(POLICY_VERSIONS); do	\
	  echo "Creating policy.$$i.$(ASSEMBLY)";	\
	  sed -e "s/@ASSEMBLY_NAME@/$(ASSEMBLY_NAME)/" -e "s/@POLICY@/$$i/" $(top_builddir)/policy.config > policy.$$i.config;	\
	  $(AL) -link:policy.$$i.config -out:policy.$$i.$(ASSEMBLY) -keyfile:gtk-sharp.snk;	\
	done

install-data-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";				\
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;			\
	  if test -n '$(POLICY_VERSIONS)'; then						\
	    for i in $(POLICY_VERSIONS); do						\
	      echo "$(GACUTIL) /i policy.$$i.$(ASSEMBLY) /f $(GACUTIL_FLAGS)";		\
	      $(GACUTIL) /i policy.$$i.$(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;	\
	    done									\
	  fi										\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then							\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";			\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;			\
	  if test -n '$(POLICY_VERSIONS)'; then						\
	    for i in $(POLICY_VERSIONS); do						\
	      echo "$(GACUTIL) /u policy.$$i.$(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";	\
	      $(GACUTIL) /u policy.$$i.$(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;	\
	    done									\
	  fi										\
	fi

