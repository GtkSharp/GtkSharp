SUBDIRS = gconf rsvg test GtkDemo pixmaps

if ENABLE_GNOME
GNOME_TARGETS=gnome-hello-world.exe canvas-example.exe fifteen.exe print.exe
GNOME_ASSEMBLY=../art/art-sharp.dll ../gnome/gnome-sharp.dll
else
GNOME_TARGETS=
GNOME_ASSEMBLY=
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

TARGETS = custom-cellrenderer.exe gtk-hello-world.exe button.exe calendar.exe subclass.exe menu.exe size.exe scribble.exe treeviewdemo.exe managedtreeviewdemo.exe testdnd.exe drawing-sample.exe $(GNOME_TARGETS) $(GLADE_TARGETS) $(VTE_TARGETS)

assemblies=../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll $(GNOME_ASSEMBLY) $(GLADE_ASSEMBLY) $(VTE_ASSEMBLY)
references=$(addprefix -r , $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)

gtk-hello-world.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC)  -out:gtk-hello-world.exe $(references) $(srcdir)/HelloWorld.cs

gnome-hello-world.exe: $(srcdir)/GnomeHelloWorld.cs $(assemblies)
	$(CSC)  -out:gnome-hello-world.exe $(references) $(srcdir)/GnomeHelloWorld.cs

canvas-example.exe: $(srcdir)/CanvasExample.cs $(assemblies)
	$(CSC)  -out:canvas-example.exe $(references) $(srcdir)/CanvasExample.cs

fifteen.exe: $(srcdir)/Fifteen.cs $(assemblies)
	$(CSC)  -out:fifteen.exe $(references) $(srcdir)/Fifteen.cs

print.exe: $(srcdir)/PrintSample.cs $(assemblies)
	$(CSC)  -out:print.exe $(references) $(srcdir)/PrintSample.cs

button.exe: $(srcdir)/ButtonApp.cs $(assemblies)
	$(CSC)  -out:button.exe $(references) $(srcdir)/ButtonApp.cs

calendar.exe: $(srcdir)/CalendarApp.cs $(assemblies)
	$(CSC)  -out:calendar.exe $(references) $(srcdir)/CalendarApp.cs

subclass.exe: $(srcdir)/Subclass.cs $(assemblies)
	$(CSC)  -out:subclass.exe $(references) $(srcdir)/Subclass.cs

menu.exe: $(srcdir)/Menu.cs $(assemblies)
	$(CSC)  -out:menu.exe $(references) $(srcdir)/Menu.cs

size.exe: $(srcdir)/Size.cs $(assemblies)
	$(CSC)  -out:size.exe $(references) $(srcdir)/Size.cs

scribble.exe: $(srcdir)/Scribble.cs $(assemblies)
	$(CSC)  -out:scribble.exe $(references) $(srcdir)/Scribble.cs

treeviewdemo.exe: $(srcdir)/TreeViewDemo.cs $(assemblies)
	$(CSC)  -out:treeviewdemo.exe $(references) $(srcdir)/TreeViewDemo.cs

managedtreeviewdemo.exe: $(srcdir)/ManagedTreeViewDemo.cs $(assemblies)
	$(CSC)  -out:managedtreeviewdemo.exe $(references) $(srcdir)/ManagedTreeViewDemo.cs

glade-viewer.exe: $(srcdir)/GladeViewer.cs $(assemblies)
	$(CSC)  -out:glade-viewer.exe $(references) $(srcdir)/GladeViewer.cs

glade-test.exe: $(srcdir)/GladeTest.cs $(srcdir)/test.glade $(assemblies)
	$(CSC)  -resource:$(srcdir)/test.glade,test.glade -out:glade-test.exe $(references) $(srcdir)/GladeTest.cs

cairo-sample.exe: $(srcdir)/CairoSample.cs $(assemblies)
	$(CSC) -out:cairo-sample.exe $(references) -r:Mono.Cairo $(srcdir)/CairoSample.cs

testdnd.exe: $(srcdir)/TestDnd.cs $(assemblies)
	$(CSC) -debug+ --unsafe -out:testdnd.exe $(references) $(srcdir)/TestDnd.cs

vte-example.exe: $(srcdir)/VteTest.cs $(assemblies)
	$(CSC) -debug+ --unsafe -out:vte-example.exe $(references) $(srcdir)/VteTest.cs

custom-cellrenderer.exe: $(srcdir)/CustomCellRenderer.cs $(assemblies)
	$(CSC) -debug+ -out:custom-cellrenderer.exe $(references) $(srcdir)/CustomCellRenderer.cs

drawing-sample.exe: $(srcdir)/DrawingSample.cs $(srcdir)/sysdraw.cs $(assemblies)
	$(CSC) -debug+ -out:drawing-sample.exe $(references) $(srcdir)/DrawingSample.cs $(srcdir)/sysdraw.cs -r:System.Drawing

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
	TreeViewDemo.cs			\
	ManagedTreeViewDemo.cs		\
	GladeViewer.cs			\
	GladeTest.cs 			\
	test.glade			\
	CairoSample.cs			\
	TestDnd.cs			\
	VteTest.cs			\
	CustomCellRenderer.cs		\
	DrawingSample.cs		\
	sysdraw.cs			\
	drawing-sample.exe.config
