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
	DemoHyperText.cs		\
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
	images/gnome-foot.png,gnome-foot.png \
	images/MonoIcon.png,MonoIcon.png \
	images/gnome-calendar.png,gnome-calendar.png \
	images/gnome-gmush.png,gnome-gmush.png \
	images/gnu-keys.png,gnu-keys.png \
	images/gnome-applets.png,gnome-applets.png \
	images/gnome-gsame.png,gnome-gsame.png \
	images/alphatest.png,alphatest.png \
	images/gnome-gimp.png,gnome-gimp.png \
	images/apple-red.png,apple-red.png \
	images/background.jpg,background.jpg \
	images/gtk-logo-rgb.gif,gtk-logo-rgb.gif \
	images/floppybuddy.gif,floppybuddy.gif

build_sources = $(addprefix $(srcdir)/, $(sources))
build_images = $(addprefix $(srcdir)/, $(images))
resources = $(addprefix /resource:, $(build_sources), $(build_images))

GtkDemo.exe: $(build_sources) $(assemblies)
	$(CSC) /debug /out:GtkDemo.exe $(build_sources) $(references) $(resources)

