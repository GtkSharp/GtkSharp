TARGETS = WidgetViewer.exe 

assemblies=../../glib/glib-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../gdk/gdk-sharp.dll ../../gtk/gtk-sharp.dll
references = $(addprefix /r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)

EXTRA_DIST = $(sources) ChangeLog

sources =			\
	TestCheckButton.cs 	\
	TestColorSelection.cs 	\
	TestFileSelection.cs 	\
	TestRadioButton.cs 	\
	TestRange.cs 		\
	TestStatusbar.cs 	\
	TestToolbar.cs 		\
	TestDialog.cs 		\
	TestFlipping.cs 	\
	TestMenus.cs 		\
	TestSizeGroup.cs	\
	TestCombo.cs 		\
	WidgetViewer.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

WidgetViewer.exe: $(build_sources) $(assemblies)
	$(CSC) /out:WidgetViewer.exe $(references) $(build_sources)

