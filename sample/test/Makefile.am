TARGETS = WidgetViewer.exe 

references=-r ../../glib/glib-sharp.dll -r ../../pango/pango-sharp.dll -r ../../atk/atk-sharp.dll -r ../../gdk/gdk-sharp.dll -r ../../gtk/gtk-sharp.dll

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

WidgetViewer.exe: $(build_sources)
	$(CSC) -o WidgetViewer.exe $(references) $(build_sources)

