references = /r:../../glib/glib-sharp.dll /r:../../pango/pango-sharp.dll /r:../../atk/atk-sharp.dll /r:../../gdk/gdk-sharp.dll /r:../../gtk/gtk-sharp.dll
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

GtkDemo.exe: $(build_sources)
	$(CSC) -g /out:GtkDemo.exe $(build_sources) $(references)

