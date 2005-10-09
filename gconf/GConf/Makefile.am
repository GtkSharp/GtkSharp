if ENABLE_GNOME
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gconf-sharp-2.0.pc
else
TARGET = 
endif
noinst_DATA = $(TARGET) $(POLICY_ASSEMBLIES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb gtk-sharp.snk $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)
DISTCLEANFILES = gconf-sharp-2.0.pc $(ASSEMBLY).config

POLICY_ASSEMBLIES = $(addsuffix .$(ASSEMBLY), $(addprefix policy., $(POLICY_VERSIONS)))
POLICY_CONFIGS = $(addsuffix .config, $(addprefix policy., $(POLICY_VERSIONS)))

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gconf-sharp

EXTRA_DIST =			\
	$(sources)		\
	$(ASSEMBLY).config.in	\
	gconf-sharp-2.0.pc.in

references = 			\
	/r:../../glib/glib-sharp.dll

sources = 			\
	ClientBase.cs 		\
	Client.cs 		\
	ChangeSet.cs 		\
	Engine.cs 		\
	_Entry.cs 		\
	NoSuchKeyException.cs 	\
	NotifyEventArgs.cs 	\
	NotifyEventHandler.cs 	\
	NotifyWrapper.cs 	\
	Value.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) ../../AssemblyInfo.cs

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) gtk-sharp.snk
	@rm -f $(ASSEMBLY).mdb
	$(CSC) $(CSFLAGS) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)

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

