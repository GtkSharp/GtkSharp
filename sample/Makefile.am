SUBDIRS = gconf rsvg test GtkDemo pixmaps gnomevfs

if ENABLE_GNOME
GNOME_TARGETS=gnome-hello-world.exe canvas-example.exe fifteen.exe print.exe
GNOME_ASSEMBLY=../art/art-sharp.dll ../gnome/gnome-sharp.dll
else
GNOME_TARGETS=
GNOME_ASSEMBLY=
endif

if ENABLE_GNOMEVFS
GNOMEVFS_TARGETS=testvfs.exe
GNOMEVFS_ASSEMBLY=../gnomevfs/gnome-vfs-sharp.dll
else
GNOMEVFS_TARGETS=
GNOMEVFS_ASSEMBLY=
endif

if ENABLE_GLADE
GLADE_TARGETS=glade-viewer.exe glade-test.exe
GLADE_ASSEMBLY=../glade/glade-sharp.dll
else
GLADE_TARGETS=
GLADE_ASSEMBLY=
endif

if ENABLE_VTE
VTE_TARGETS=vte-example.exe
VTE_ASSEMBLY=../vte/vte-sharp.dll
else
VTE_TARGETS=
VTE_ASSEMBLY=
endif

TARGETS = custom-notebook.exe drawing-sample.exe custom-widget.exe custom-cellrenderer.exe gtk-hello-world.exe button.exe calendar.exe subclass.exe menu.exe size.exe scribble.exe scribble-xinput.exe treeviewdemo.exe managedtreeviewdemo.exe nodeviewdemo.exe testdnd.exe actions.exe $(GNOMEVFS_TARGETS) $(GNOME_TARGETS) $(GLADE_TARGETS) $(VTE_TARGETS)

assemblies=../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll $(GNOMEVFS_ASSEMBLY) $(GNOME_ASSEMBLY) $(GLADE_ASSEMBLY) $(VTE_ASSEMBLY)
references=$(addprefix /r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)

gtk-hello-world.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC)  /out:gtk-hello-world.exe $(references) $(srcdir)/HelloWorld.cs

gnome-hello-world.exe: $(srcdir)/GnomeHelloWorld.cs $(assemblies)
	$(CSC)  /out:gnome-hello-world.exe $(references) $(srcdir)/GnomeHelloWorld.cs

canvas-example.exe: $(srcdir)/CanvasExample.cs $(assemblies)
	$(CSC)  /out:canvas-example.exe $(references) $(srcdir)/CanvasExample.cs

fifteen.exe: $(srcdir)/Fifteen.cs $(assemblies)
	$(CSC)  /out:fifteen.exe $(references) $(srcdir)/Fifteen.cs

print.exe: $(srcdir)/PrintSample.cs $(assemblies)
	$(CSC)  /out:print.exe $(references) $(srcdir)/PrintSample.cs

button.exe: $(srcdir)/ButtonApp.cs $(assemblies)
	$(CSC)  /out:button.exe $(references) $(srcdir)/ButtonApp.cs

calendar.exe: $(srcdir)/CalendarApp.cs $(assemblies)
	$(CSC)  /out:calendar.exe $(references) $(srcdir)/CalendarApp.cs

subclass.exe: $(srcdir)/Subclass.cs $(assemblies)
	$(CSC)  /out:subclass.exe $(references) $(srcdir)/Subclass.cs

menu.exe: $(srcdir)/Menu.cs $(assemblies)
	$(CSC)  /out:menu.exe $(references) $(srcdir)/Menu.cs

size.exe: $(srcdir)/Size.cs $(assemblies)
	$(CSC)  /out:size.exe $(references) $(srcdir)/Size.cs

scribble.exe: $(srcdir)/Scribble.cs $(assemblies)
	$(CSC)  /out:scribble.exe $(references) $(srcdir)/Scribble.cs

scribble-xinput.exe: $(srcdir)/ScribbleXInput.cs $(assemblies)
	$(CSC)  /out:scribble-xinput.exe $(references) $(srcdir)/ScribbleXInput.cs

treeviewdemo.exe: $(srcdir)/TreeViewDemo.cs $(assemblies)
	$(CSC)  /out:treeviewdemo.exe $(references) $(srcdir)/TreeViewDemo.cs

managedtreeviewdemo.exe: $(srcdir)/ManagedTreeViewDemo.cs $(assemblies)
	$(CSC)  /out:managedtreeviewdemo.exe $(references) $(srcdir)/ManagedTreeViewDemo.cs

nodeviewdemo.exe: $(srcdir)/NodeViewDemo.cs $(assemblies)
	$(CSC)  /out:nodeviewdemo.exe $(references) $(srcdir)/NodeViewDemo.cs

glade-viewer.exe: $(srcdir)/GladeViewer.cs $(assemblies)
	$(CSC)  /out:glade-viewer.exe $(references) $(srcdir)/GladeViewer.cs

glade-test.exe: $(srcdir)/GladeTest.cs $(srcdir)/test.glade $(assemblies)
	$(CSC)  /resource:$(srcdir)/test.glade,test.glade /out:glade-test.exe $(references) $(srcdir)/GladeTest.cs

cairo-sample.exe: $(srcdir)/CairoSample.cs $(srcdir)/GtkCairo.cs $(assemblies)
	$(CSC) /out:cairo-sample.exe $(references) /r:Mono.Cairo $(srcdir)/CairoSample.cs $(srcdir)/GtkCairo.cs

testdnd.exe: $(srcdir)/TestDnd.cs $(assemblies)
	$(CSC) /debug /unsafe /out:testdnd.exe $(references) $(srcdir)/TestDnd.cs

vte-example.exe: $(srcdir)/VteTest.cs $(assemblies)
	$(CSC) /debug /unsafe /out:vte-example.exe $(references) $(srcdir)/VteTest.cs

custom-cellrenderer.exe: $(srcdir)/CustomCellRenderer.cs $(assemblies)
	$(CSC) /debug /out:custom-cellrenderer.exe $(references) $(srcdir)/CustomCellRenderer.cs

drawing-sample.exe: $(srcdir)/DrawingSample.cs $(srcdir)/sysdraw.cs $(assemblies)
	$(CSC) /debug /out:drawing-sample.exe $(references) $(srcdir)/DrawingSample.cs $(srcdir)/sysdraw.cs /r:System.Drawing

custom-widget.exe: $(srcdir)/CustomWidget.cs $(assemblies)
	$(CSC) /debug /out:custom-widget.exe $(references) $(srcdir)/CustomWidget.cs

actions.exe: $(srcdir)/Actions.cs
	$(CSC) /debug /unsafe /out:actions.exe $(references) $(srcdir)/Actions.cs

custom-notebook.exe: $(srcdir)/CustomNotebook.cs $(assemblies)
	$(CSC) /debug /out:custom-notebook.exe $(references) $(srcdir)/CustomNotebook.cs

testvfs.exe: $(srcdir)/TestVfs.cs $(assemblies)
	$(CSC) /debug /out:testvfs.exe $(references) $(srcdir)/TestVfs.cs

EXTRA_DIST = 				\
	HelloWorld.cs			\
	GnomeHelloWorld.cs		\
	CanvasExample.cs		\
	Fifteen.cs			\
	PrintSample.cs			\
	ButtonApp.cs			\
	CalendarApp.cs			\
	Subclass.cs			\
	Menu.cs				\
	Size.cs				\
	Scribble.cs			\
	ScribbleXInput.cs		\
	TreeViewDemo.cs			\
	ManagedTreeViewDemo.cs		\
	NodeViewDemo.cs			\
	GladeViewer.cs			\
	GladeTest.cs 			\
	test.glade			\
	CairoSample.cs			\
	TestDnd.cs			\
	VteTest.cs			\
	CustomCellRenderer.cs		\
	DrawingSample.cs		\
	sysdraw.cs			\
	drawing-sample.exe.config	\
	cairo-sample.exe.config		\
	CustomWidget.cs			\
	Actions.cs			\
	CustomNotebook.cs		\
	TestVfs.cs
