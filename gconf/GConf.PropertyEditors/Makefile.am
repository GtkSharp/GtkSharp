if ENABLE_GNOME
TARGET=$(ASSEMBLY)
else
TARGET=
endif

ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_NAME = gconf-sharp-peditors
noinst_DATA = $(TARGET)
CLEANFILES = $(ASSEMBLY) gtk-sharp.snk
DISTCLEANFILES = AssemblyInfo.cs

EXTRA_DIST =				\
	AssemblyInfo.cs.in		\
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
	PropertyEditorOptionMenu.cs 	\
	PropertyEditorRadioButton.cs 	\
	PropertyEditorSpinButton.cs 	\
	PropertyEditorToggleButton.cs 	\
	EditorShell.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) gtk-sharp.snk
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)

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

