assemblies = \
	$(top_builddir)/glib/glib-sharp.dll 	\
	$(top_builddir)/gio/gio-sharp.dll	\
	$(top_builddir)/cairo/cairo-sharp.dll	\
	$(top_builddir)/pango/pango-sharp.dll	\
	$(top_builddir)/atk/atk-sharp.dll	\
	$(top_builddir)/gdk/gdk-sharp.dll	\
	$(top_builddir)/gtk/gtk-sharp.dll

references = $(addprefix -r:, $(assemblies))
TARGETS = GtkDemo.exe
DEBUGS = $(addsuffix .mdb, $(TARGETS)) 
CLEANFILES = $(TARGETS) $(DEBUGS)
noinst_SCRIPTS = $(TARGETS)

EXTRA_DIST = $(sources) $(resource_files)

sources = \
	DemoApplicationWindow.cs	\
	DemoAttribute.cs		\
	DemoButtonBox.cs		\
	DemoClipboard.cs		\
	DemoColorSelection.cs	\
	DemoCssBasics.cs		\
	DemoDialog.cs			\
	DemoDrawingArea.cs		\
	DemoEditableCells.cs	\
	DemoEntryCompletion.cs	\
	DemoExpander.cs			\
	DemoHyperText.cs		\
	DemoIconView.cs			\
	DemoImages.cs			\
	DemoListStore.cs		\
	DemoMain.cs				\
	DemoMenus.cs			\
	DemoPanes.cs			\
	DemoPixbuf.cs			\
	DemoRotatedText.cs		\
	DemoSizeGroup.cs		\
	DemoSpinner.cs			\
	DemoStockBrowser.cs		\
	DemoTextView.cs			\
	DemoThemingStyleClasses.cs	\
	DemoTreeStore.cs		\
	DemoUIManager.cs		\
	DemoPrinting.cs

resource_ids = \
	css/css_basics.css,css_basics.css \
	css/reset.css,reset.css \
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
	images/floppybuddy.gif,floppybuddy.gif \
	theming.ui,theming.ui

resource_files = \
	css/css_basics.css \
	css/reset.css \
	images/gnome-foot.png \
	images/MonoIcon.png \
	images/gnome-calendar.png \
	images/gnome-gmush.png \
	images/gnu-keys.png \
	images/gnome-applets.png \
	images/gnome-gsame.png \
	images/alphatest.png \
	images/gnome-gimp.png \
	images/apple-red.png \
	images/background.jpg \
	images/gtk-logo-rgb.gif \
	images/floppybuddy.gif \
	theming.ui

build_sources = $(addprefix $(srcdir)/, $(sources))
build_resources = $(addprefix $(srcdir)/, $(resource_ids))
resources = $(addprefix -resource:, $(build_sources), $(build_resources))

GtkDemo.exe: $(build_sources) $(assemblies)
	$(CSC) $(CSFLAGS) -out:GtkDemo.exe $(build_sources) $(references) $(resources)

