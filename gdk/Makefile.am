SUBDIRS = . glue

API = gdk-api.xml
RAW_API = gdk-api.raw
ADDITIONAL_API = gdk-symbols.xml
INCLUDE_API = ../pango/pango-api.xml
METADATA = Gdk.metadata
ASSEMBLY_NAME = gdk-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
gapi_DATA = $(API) $(ADDITIONAL_API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) glue/generated.c gtk-sharp.pub
DISTCLEANFILES = $(ASSEMBLY).config AssemblyInfo.cs

sources = 			\
	EventButton.cs		\
	EventClient.cs		\
	EventConfigure.cs	\
	EventCrossing.cs	\
	Event.cs		\
	EventDND.cs		\
	EventExpose.cs		\
	EventFocus.cs		\
	EventKey.cs		\
	EventMotion.cs		\
	EventProperty.cs	\
	EventProximity.cs	\
	EventScroll.cs		\
	EventSelection.cs	\
	EventSetting.cs		\
	EventVisibility.cs	\
	EventWindowState.cs	\
	Key.cs			\
	Size.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs = 			\
	Atom.custom		\
	Color.custom		\
	DeviceAxis.custom	\
	Display.custom		\
	DisplayManager.custom	\
	DragContext.custom	\
	Drawable.custom		\
	Global.custom		\
	Pixbuf.custom		\
	PixbufLoader.custom	\
	Point.custom		\
	Rectangle.custom	\
	Region.custom		\
	Screen.custom		\
	Selection.custom	\
	Window.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	makefile.win32		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(RAW_API)		\
	$(ADDITIONAL_API)	\
	$(METADATA)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) $(srcdir)/$(ADDITIONAL_API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API) 			\
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) 	\
	--gluelib-name=gdksharpglue --glue-filename=glue/generated.c						\
	&& touch generated-stamp

gtk-sharp.pub: $(top_srcdir)/gtk-sharp.pub
	cp $(top_srcdir)/gtk-sharp.pub .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.pub
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

install-data-local:
	echo "$(GACUTIL) /i $(ASSEMBLY) /f /root $(DESTDIR)$(libdir)";	\
	$(GACUTIL) /i $(ASSEMBLY) /f /root $(DESTDIR)$(libdir) || exit 1;

uninstall-local:
	echo "$(GACUTIL) /u $(ASSEMBLY_NAME) /root $(DESTDIR)$(libdir)";	\
	$(GACUTIL) /u $(ASSEMBLY_NAME) /root $(DESTDIR)$(libdir) || exit 1;

