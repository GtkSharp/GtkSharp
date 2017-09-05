SUBDIRS = test GtkDemo pixmaps valtest opaquetest gio gtk-gio

if ENABLE_DOTNET
DOTNET_TARGETS=drawing-sample.exe
DOTNET_ASSEMBLY=../gtkdotnet/gtk-dotnet.dll
else
DOTNET_TARGETS=
DOTNET_ASSEMBLY=
endif

TARGETS = gtk-hello-world.exe async-sample.exe button.exe calendar.exe subclass.exe menu.exe treeviewdemo.exe managedtreeviewdemo.exe nodeviewdemo.exe treemodeldemo.exe actions.exe spawn.exe assistant.exe registerprop.exe gexceptiontest.exe native-instantiation.exe polarfixed.exe cairo-sample.exe scribble.exe testdnd.exe custom-cellrenderer.exe custom-widget.exe custom-scrollable.exe cairo-png.exe variantdemo.exe #scribble-xinput.exe $(DOTNET_TARGETS)

DEBUGS = $(addsuffix .mdb, $(TARGETS))

assemblies = 					\
	$(top_builddir)/glib/glib-sharp.dll 	\
	$(top_builddir)/gio/gio-sharp.dll 	\
	$(top_builddir)/cairo/cairo-sharp.dll 	\
	$(top_builddir)/pango/pango-sharp.dll 	\
	$(top_builddir)/atk/atk-sharp.dll 	\
	$(top_builddir)/gdk/gdk-sharp.dll 	\
	$(top_builddir)/gtk/gtk-sharp.dll 

references=$(addprefix -r:, $(assemblies))

noinst_SCRIPTS = $(TARGETS)
CLEANFILES = $(TARGETS) $(DEBUGS)

gtk-hello-world.exe: $(srcdir)/HelloWorld.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:gtk-hello-world.exe $(references) $(srcdir)/HelloWorld.cs

async-sample.exe: $(srcdir)/AsyncSample.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:async-sample.exe $(references) $(srcdir)/AsyncSample.cs

button.exe: $(srcdir)/ButtonApp.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:button.exe $(references) $(srcdir)/ButtonApp.cs

calendar.exe: $(srcdir)/CalendarApp.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:calendar.exe $(references) $(srcdir)/CalendarApp.cs

subclass.exe: $(srcdir)/Subclass.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:subclass.exe $(references) $(srcdir)/Subclass.cs

menu.exe: $(srcdir)/Menu.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:menu.exe $(references) $(srcdir)/Menu.cs

native-instantiation.exe: $(srcdir)/NativeInstantiationTest.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:native-instantiation.exe $(references) $(srcdir)/NativeInstantiationTest.cs

scribble.exe: $(srcdir)/Scribble.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:scribble.exe $(references) $(srcdir)/Scribble.cs

scribble-xinput.exe: $(srcdir)/ScribbleXInput.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:scribble-xinput.exe $(references) $(srcdir)/ScribbleXInput.cs

treeviewdemo.exe: $(srcdir)/TreeViewDemo.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:treeviewdemo.exe $(references) $(srcdir)/TreeViewDemo.cs

managedtreeviewdemo.exe: $(srcdir)/ManagedTreeViewDemo.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:managedtreeviewdemo.exe $(references) $(srcdir)/ManagedTreeViewDemo.cs

nodeviewdemo.exe: $(srcdir)/NodeViewDemo.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:nodeviewdemo.exe $(references) $(srcdir)/NodeViewDemo.cs

treemodeldemo.exe: $(srcdir)/TreeModelDemo.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:treemodeldemo.exe $(references) $(srcdir)/TreeModelDemo.cs

cairo-sample.exe: $(srcdir)/CairoSample.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:cairo-sample.exe $(references) $(srcdir)/CairoSample.cs

testdnd.exe: $(srcdir)/TestDnd.cs $(assemblies)
	$(CSC) $(CSFLAGS) -unsafe -out:testdnd.exe $(references) $(srcdir)/TestDnd.cs

custom-cellrenderer.exe: $(srcdir)/CustomCellRenderer.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:custom-cellrenderer.exe $(references) $(srcdir)/CustomCellRenderer.cs

dotnet_references = $(references) $(addprefix -r:, $(DOTNET_ASSEMBLY)) -r:System.Drawing.dll

drawing-sample.exe: $(srcdir)/DrawingSample.cs $(assemblies) $(DOTNET_ASSEMBLIES)
	$(CSC) $(CSFLAGS) -out:drawing-sample.exe $(dotnet_references) $(srcdir)/DrawingSample.cs

custom-widget.exe: $(srcdir)/CustomWidget.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:custom-widget.exe $(references) $(srcdir)/CustomWidget.cs

custom-scrollable.exe: $(srcdir)/CustomScrollableWidget.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:custom-scrollable.exe $(references) $(srcdir)/CustomScrollableWidget.cs

actions.exe: $(srcdir)/Actions.cs
	$(CSC) $(CSFLAGS) -unsafe -out:actions.exe $(references) $(srcdir)/Actions.cs

polarfixed.exe: $(srcdir)/PolarFixed.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:polarfixed.exe $(references) $(srcdir)/PolarFixed.cs

spawn.exe: $(srcdir)/SpawnTests.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:spawn.exe $(references) $(srcdir)/SpawnTests.cs

assistant.exe: $(srcdir)/Assistant.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:assistant.exe $(references) $(srcdir)/Assistant.cs

registerprop.exe: $(srcdir)/PropertyRegistration.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:registerprop.exe $(references) $(srcdir)/PropertyRegistration.cs

gexceptiontest.exe: $(srcdir)/GExceptionTest.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:gexceptiontest.exe $(references) $(srcdir)/GExceptionTest.cs

cairo-png.exe: $(srcdir)/CairoPng.cs $(top_builddir)/cairo/cairo-sharp.dll
	$(CSC) $(CSFLAGS) -out:cairo-png.exe -r:$(top_builddir)/cairo/cairo-sharp.dll $(srcdir)/CairoPng.cs

variantdemo.exe: $(srcdir)/VariantDemo.cs $(assemblies)
	$(CSC) $(CSFLAGS) -out:variantdemo.exe $(references) $(srcdir)/VariantDemo.cs

EXTRA_DIST = 				\
	HelloWorld.cs			\
	Assistant.cs			\
	AsyncSample.cs			\
	ButtonApp.cs			\
	CalendarApp.cs			\
	Subclass.cs			\
	Menu.cs				\
	Scribble.cs			\
	ScribbleXInput.cs		\
	SpawnTests.cs			\
	TreeModelDemo.cs		\
	TreeViewDemo.cs			\
	ManagedTreeViewDemo.cs		\
	NativeInstantiationTest.cs	\
	NodeViewDemo.cs			\
	GExceptionTest.cs		\
	CairoPng.cs				\
	CairoSample.cs			\
	TestDnd.cs			\
	CustomCellRenderer.cs		\
	DrawingSample.cs		\
	CustomWidget.cs			\
	CustomScrollableWidget.cs	\
	Actions.cs			\
	PropertyRegistration.cs		\
	PolarFixed.cs			\
	VariantDemo.cs

