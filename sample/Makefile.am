SUBDIRS = test GtkDemo pixmaps valtest opaquetest

if ENABLE_MONO_CAIRO
cairo_ref=-r:$(top_builddir)/cairo/Mono.Cairo.dll
else
cairo_ref=$(MONO_CAIRO_LIBS)
endif

if ENABLE_GLADE
GLADE_TARGETS=glade-viewer.exe glade-test.exe
GLADE_ASSEMBLY=../glade/glade-sharp.dll
else
GLADE_TARGETS=
GLADE_ASSEMBLY=
endif

if ENABLE_DOTNET
DOTNET_TARGETS=drawing-sample.exe
DOTNET_ASSEMBLY=../gtkdotnet/gtk-dotnet.dll
else
DOTNET_TARGETS=
DOTNET_ASSEMBLY=
endif

TARGETS = polarfixed.exe custom-widget.exe custom-cellrenderer.exe gtk-hello-world.exe button.exe calendar.exe subclass.exe menu.exe size.exe scribble.exe scribble-xinput.exe treeviewdemo.exe managedtreeviewdemo.exe nodeviewdemo.exe treemodeldemo.exe testdnd.exe actions.exe spawn.exe assistant.exe registerprop.exe gexceptiontest.exe cairo-sample.exe $(GLADE_TARGETS) $(DOTNET_TARGETS)

DEBUGS = $(addsuffix .mdb, $(TARGETS))

assemblies=../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll $(GLADE_ASSEMBLY)
references=$(addprefix /r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

gtk-hello-world.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC)  /out:gtk-hello-world.exe $(references) $(srcdir)/HelloWorld.cs

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

treemodeldemo.exe: $(srcdir)/TreeModelDemo.cs $(assemblies)
	$(CSC)  /out:treemodeldemo.exe $(references) $(srcdir)/TreeModelDemo.cs

glade-viewer.exe: $(srcdir)/GladeViewer.cs $(assemblies)
	$(CSC)  /out:glade-viewer.exe $(references) $(srcdir)/GladeViewer.cs

glade-test.exe: $(srcdir)/GladeTest.cs $(srcdir)/test.glade $(assemblies)
	$(CSC)  /resource:$(srcdir)/test.glade,test.glade /out:glade-test.exe $(references) $(srcdir)/GladeTest.cs

cairo-sample.exe: $(srcdir)/CairoSample.cs $(assemblies)
	$(CSC) /out:cairo-sample.exe $(references) $(cairo_ref) $(srcdir)/CairoSample.cs

testdnd.exe: $(srcdir)/TestDnd.cs $(assemblies)
	$(CSC) /debug /unsafe /out:testdnd.exe $(references) $(srcdir)/TestDnd.cs

custom-cellrenderer.exe: $(srcdir)/CustomCellRenderer.cs $(assemblies)
	$(CSC) /debug /out:custom-cellrenderer.exe $(references) $(srcdir)/CustomCellRenderer.cs

dotnet_references = $(references) $(addprefix -r:, $(DOTNET_ASSEMBLY)) -r:System.Drawing.dll

drawing-sample.exe: $(srcdir)/DrawingSample.cs $(assemblies) $(DOTNET_ASSEMBLIES)
	$(CSC) /debug /out:drawing-sample.exe $(dotnet_references) $(srcdir)/DrawingSample.cs

custom-widget.exe: $(srcdir)/CustomWidget.cs $(assemblies)
	$(CSC) /debug /out:custom-widget.exe $(references) $(srcdir)/CustomWidget.cs

actions.exe: $(srcdir)/Actions.cs
	$(CSC) /debug /unsafe /out:actions.exe $(references) $(srcdir)/Actions.cs

polarfixed.exe: $(srcdir)/PolarFixed.cs $(assemblies)
	$(CSC) /debug /out:polarfixed.exe $(references) $(srcdir)/PolarFixed.cs

spawn.exe: $(srcdir)/SpawnTests.cs $(assemblies)
	$(CSC)  /out:spawn.exe $(references) $(srcdir)/SpawnTests.cs

assistant.exe: $(srcdir)/Assistant.cs $(assemblies)
	$(CSC)  /out:assistant.exe $(references) $(srcdir)/Assistant.cs

registerprop.exe: $(srcdir)/PropertyRegistration.cs $(assemblies)
	$(CSC)  /out:registerprop.exe $(references) $(srcdir)/PropertyRegistration.cs

gexceptiontest.exe: $(srcdir)/GExceptionTest.cs $(assemblies)
	$(CSC)  /out:gexceptiontest.exe $(references) $(srcdir)/GExceptionTest.cs

EXTRA_DIST = 				\
	HelloWorld.cs			\
	Assistant.cs			\
	ButtonApp.cs			\
	CalendarApp.cs			\
	Subclass.cs			\
	Menu.cs				\
	Size.cs				\
	Scribble.cs			\
	ScribbleXInput.cs		\
	SpawnTests.cs			\
	TreeModelDemo.cs		\
	TreeViewDemo.cs			\
	ManagedTreeViewDemo.cs		\
	NodeViewDemo.cs			\
	GExceptionTest.cs		\
	GladeViewer.cs			\
	GladeTest.cs 			\
	test.glade			\
	CairoSample.cs			\
	TestDnd.cs			\
	CustomCellRenderer.cs		\
	DrawingSample.cs		\
	CustomWidget.cs			\
	Actions.cs			\
	PropertyRegistration.cs		\
	PolarFixed.cs

