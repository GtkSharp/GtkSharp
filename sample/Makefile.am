SUBDIRS = gconf rsvg test GtkDemo pixmaps

if ENABLE_GNOME
GNOME_TARGETS=gnome-hello-world.exe canvas-example.exe fifteen.exe print.exe
GNOME_ASSEMBLY=-r ../art/art-sharp.dll -r ../gnome/gnome-sharp.dll
else
GNOME_TARGETS=
GNOME_ASSEMBLY=
endif

if ENABLE_GLADE
GLADE_TARGETS=glade-viewer.exe glade-test.exe
GLADE_ASSEMBLY=-r ../glade/glade-sharp.dll
else
GLADE_TARGETS=
GLADE_ASSEMBLY=
endif

if ENABLE_VTE
VTE_TARGETS=vte-example.exe
VTE_ASSEMBLY=-r ../vte/vte-sharp.dll
else
VTE_TARGETS=
VTE_ASSEMBLY=
endif

TARGETS = gtk-hello-world.exe button.exe calendar.exe subclass.exe menu.exe size.exe scribble.exe treeviewdemo.exe managedtreeviewdemo.exe testdnd.exe $(GNOME_TARGETS) $(GLADE_TARGETS) $(VTE_TARGETS)

references=-r ../glib/glib-sharp.dll -r ../pango/pango-sharp.dll -r ../atk/atk-sharp.dll -r ../gdk/gdk-sharp.dll -r ../gtk/gtk-sharp.dll $(GNOME_ASSEMBLY) $(GLADE_ASSEMBLY) $(VTE_ASSEMBLY)

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS)

gtk-hello-world.exe: $(srcdir)/HelloWorld.cs
	$(CSC)  -out:gtk-hello-world.exe $(references) $(srcdir)/HelloWorld.cs

gnome-hello-world.exe: $(srcdir)/GnomeHelloWorld.cs
	$(CSC)  -out:gnome-hello-world.exe $(references) $(srcdir)/GnomeHelloWorld.cs

canvas-example.exe: $(srcdir)/CanvasExample.cs
	$(CSC)  -out:canvas-example.exe $(references) $(srcdir)/CanvasExample.cs

fifteen.exe: $(srcdir)/Fifteen.cs
	$(CSC)  -out:fifteen.exe $(references) $(srcdir)/Fifteen.cs

print.exe: $(srcdir)/PrintSample.cs
	$(CSC)  -out:print.exe $(references) $(srcdir)/PrintSample.cs

button.exe: $(srcdir)/ButtonApp.cs
	$(CSC)  -out:button.exe $(references) $(srcdir)/ButtonApp.cs

calendar.exe: $(srcdir)/CalendarApp.cs
	$(CSC)  -out:calendar.exe $(references) $(srcdir)/CalendarApp.cs

subclass.exe: $(srcdir)/Subclass.cs
	$(CSC)  -out:subclass.exe $(references) $(srcdir)/Subclass.cs

menu.exe: $(srcdir)/Menu.cs
	$(CSC)  -out:menu.exe $(references) $(srcdir)/Menu.cs

size.exe: $(srcdir)/Size.cs
	$(CSC)  -out:size.exe $(references) $(srcdir)/Size.cs

scribble.exe: $(srcdir)/Scribble.cs
	$(CSC)  -out:scribble.exe $(references) $(srcdir)/Scribble.cs

treeviewdemo.exe: $(srcdir)/TreeViewDemo.cs
	$(CSC)  -out:treeviewdemo.exe $(references) $(srcdir)/TreeViewDemo.cs

managedtreeviewdemo.exe: $(srcdir)/ManagedTreeViewDemo.cs
	$(CSC)  -out:managedtreeviewdemo.exe $(references) $(srcdir)/ManagedTreeViewDemo.cs

glade-viewer.exe: $(srcdir)/GladeViewer.cs
	$(CSC)  -out:glade-viewer.exe $(references) $(srcdir)/GladeViewer.cs

glade-test.exe: $(srcdir)/GladeTest.cs $(srcdir)/test.glade
	$(CSC)  -resource:$(srcdir)/test.glade -out:glade-test.exe $(references) $(srcdir)/GladeTest.cs

cairo-sample.exe: $(srcdir)/CairoSample.cs
	$(CSC) -out:cairo-sample.exe $(references) -r:Mono.Cairo $(srcdir)/CairoSample.cs

testdnd.exe: $(srcdir)/TestDnd.cs
	$(CSC) -g --unsafe -o testdnd.exe $(references) $(srcdir)/TestDnd.cs

vte-example.exe: $(srcdir)/VteTest.cs
	$(CSC) -g --unsafe -o vte-example.exe $(references) $(srcdir)/VteTest.cs

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
	VteTest.cs

