if ENABLE_GNOME
TARGET=$(ASSEMBLY)
else
TARGET=
endif

ASSEMBLY = gconf-sharp-peditors.dll
assemblydir = $(libdir)
assembly_DATA = $(TARGET)
CLEANFILES = $(ASSEMBLY)
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

$(ASSEMBLY): $(build_sources)
	$(CSC) /out:$(ASSEMBLY) /target:library $(references) $(build_sources)

