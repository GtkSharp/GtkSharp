assemblies = ../../glib/glib-sharp.dll ../../pango/pango-sharp.dll ../../atk/atk-sharp.dll ../../gdk/gdk-sharp.dll ../../gtk/gtk-sharp.dll
references = $(addprefix /r:, $(assemblies))
TARGETS = GtkDemo.exe
CLEANFILES = $(TARGETS)
noinst_SCRIPTS = $(TARGETS)

EXTRA_DIST = $(sources)

sources = 					\
	DemoApplicationWindow.cs	\
	DemoButtonBox.cs		\
	DemoColorSelection.cs		\
	DemoDialog.cs			\
	DemoDrawingArea.cs		\
	DemoEditableCells.cs		\
	DemoImages.cs			\
	DemoItemFactory.cs		\
	DemoListStore.cs		\
	DemoMain.cs			\
	DemoMenus.cs			\
	DemoPanes.cs			\
	DemoPixbuf.cs			\
	DemoSizeGroup.cs		\
	DemoStockBrowser.cs		\
	DemoTextView.cs		\
	DemoTreeStore.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

GtkDemo.exe: $(build_sources) $(assemblies)
	$(CSC) /debug /out:GtkDemo.exe $(build_sources) $(references)

