if ENABLE_GNOME
TARGET=$(ASSEMBLY)
else
TARGET=
endif

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gconf-sharp-peditors
noinst_DATA = $(TARGET) $(POLICY_ASSEMBLIES)
CLEANFILES = $(ASSEMBLY) $(ASSEMBLY).mdb gtk-sharp.snk $(POLICY_ASSEMBLIES) $(POLICY_CONFIGS)

POLICY_ASSEMBLIES = $(addsuffix .$(ASSEMBLY), $(addprefix policy., $(POLICY_VERSIONS)))
POLICY_CONFIGS = $(addsuffix .config, $(addprefix policy., $(POLICY_VERSIONS)))

EXTRA_DIST =				\
	PropertyEditorColorPicker.cs	\
	$(sources)

references = 			\
	/r:../../glib/glib-sharp.dll /r:../../pango/pango-sharp.dll /r:../../atk/atk-sharp.dll /r:../../gdk/gdk-sharp.dll /r:../../gtk/gtk-sharp.dll /r:../../art/art-sharp.dll /r:../../gnome/gnome-sharp.dll /r:../../glade/glade-sharp.dll /r:../GConf/gconf-sharp.dll

sources = 				\
	PropertyEditor.cs 		\
	PropertyEditorBool.cs 		\
	PropertyEditorEnum.cs 		\
	PropertyEditorEntry.cs 		\
	PropertyEditorFileEntry.cs 	\
	PropertyEditorOptionMenu.cs	\
	PropertyEditorRadioButton.cs 	\
	PropertyEditorSpinButton.cs 	\
	PropertyEditorToggleButton.cs 	\
	EditorShell.cs

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

