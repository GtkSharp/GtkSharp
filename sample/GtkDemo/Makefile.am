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

images = \
/resource:$(srcdir)/images/gnome-foot.png,gnome-foot.png \
/resource:$(srcdir)/images/MonoIcon.png,MonoIcon.png \
/resource:$(srcdir)/images/gnome-calendar.png,gnome-calendar.png \
/resource:$(srcdir)/images/gnome-gmush.png,gnome-mush.png \
/resource:$(srcdir)/images/gnu-keys.png,gnu-keys.png \
/resource:$(srcdir)/images/gnome-applets.png,gnome-applets.png \
/resource:$(srcdir)/images/gnome-gsame.png,gnome-gsame.png \
/resource:$(srcdir)/images/alphatest.png,alphatest.png \
/resource:$(srcdir)/images/gnome-gimp.png,gnome-gimp.png \
/resource:$(srcdir)/images/apple-red.png,apple-red.png \
/resource:$(srcdir)/images/background.jpg,background.jpg \
/resource:$(srcdir)/images/gtk-logo-rgb.gif,gtk-logo-rgb.gif \
/resource:$(srcdir)/images/floppybuddy.gif,floppybuddy.gif

build_sources = $(addprefix $(srcdir)/, $(sources))

GtkDemo.exe: $(build_sources) $(assemblies)
	$(CSC) /debug /out:GtkDemo.exe $(build_sources) $(references) $(images)

