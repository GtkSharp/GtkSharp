SUBDIRS = . glue

API = gdk-api.xml
RAW_API = gdk-api.raw
ADDITIONAL_API = gdk-symbols.xml
INCLUDE_API = ../pango/pango-api.xml
METADATA = Gdk.metadata
ASSEMBLY_NAME = gdk-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll

TARGET = $(ASSEMBLY)
ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi-2.0
noinst_DATA = $(ASSEMBLY) $(ASSEMBLY).config
gapi_DATA = $(API) $(ADDITIONAL_API)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) glue/generated.c gtk-sharp.snk
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
	Size.cs			\
	TextProperty.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs = 			\
	Atom.custom		\
	Color.custom		\
	Device.custom		\
	DeviceAxis.custom	\
	Display.custom		\
	DisplayManager.custom	\
	DragContext.custom	\
	Drawable.custom		\
	Global.custom		\
	Keymap.custom		\
	Pixmap.custom		\
	Pixbuf.custom		\
	PixbufLoader.custom	\
	Pixdata.custom		\
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
	rm -f generated/* &&	\
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) $(srcdir)/$(ADDITIONAL_API) 			\
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) 	\
	--gluelib-name=gdksharpglue --glue-filename=glue/generated.c						\
	&& touch generated-stamp

gtk-sharp.snk: $(top_srcdir)/gtk-sharp.snk
	cp $(top_srcdir)/gtk-sharp.snk .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.snk
	$(CSC) /nowarn:0612 /unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) $(GENERATED_SOURCES)

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f $(GACUTIL_FLAGS) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;	\
	fi

