SUBDIRS = glue

if ENABLE_GNOME
TARGET = $(ASSEMBLY) $(ASSEMBLY).config
APIS = $(API)

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gnome-sharp.pc

else
TARGET = 
APIS = 
endif

API = gnome-api.xml
RAW_API = gnome-api.raw
INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml $(top_srcdir)/gdk/gdk-symbols.xml ../gtk/gtk-api.xml $(top_srcdir)/gtk/gtk-symbols.xml ../art/art-api.xml
METADATA = Gnome.metadata
ASSEMBLY_NAME = gnome-sharp
references = /r:../glib/glib-sharp.dll /r:../pango/pango-sharp.dll /r:../atk/atk-sharp.dll /r:../gdk/gdk-sharp.dll /r:../gtk/gtk-sharp.dll /r:../art/art-sharp.dll

ASSEMBLY = $(ASSEMBLY_NAME).dll
gapidir = $(datadir)/gapi
noinst_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API) gtk-sharp.pub

DISTCLEANFILES = gnome-sharp.pc $(ASSEMBLY).config AssemblyInfo.cs

sources =					\
	CanvasProxy.cs				\
	GtkSharp.BoundsHandler.cs		\
	GtkSharp.DrawHandler.cs			\
	GtkSharp.PointHandler.cs		\
	GtkSharp.RenderHandler.cs		\
	GtkSharp.UpdateHandler.cs		\
	Modules.cs				\
	voidObjectAffineSVPintSignal.cs

build_sources = $(addprefix $(srcdir)/, $(sources)) AssemblyInfo.cs

customs =					\
	App.custom				\
	CanvasBpath.custom			\
	CanvasClipgroup.custom			\
	Canvas.custom				\
	CanvasEllipse.custom			\
	CanvasGroup.custom			\
	CanvasItem.custom			\
	CanvasLine.custom			\
	CanvasPixbuf.custom			\
	CanvasPoints.custom			\
	CanvasPolygon.custom			\
	CanvasRect.custom			\
	CanvasRE.custom				\
	CanvasRichText.custom			\
	CanvasShape.custom			\
	CanvasText.custom			\
	CanvasWidget.custom			\
	Druid.custom				\
	Font.custom				\
	FontFamily.custom			\
	IconList.custom				\
	IconTextItem.custom			\
	IconTheme.custom			\
	Print.custom				\
	PrintDialog.custom			\
	PrintJob.custom				\
	Program.custom				\
	Scores.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(ASSEMBLY).config.in	\
	AssemblyInfo.cs.in	\
	$(METADATA)		\
	$(customs)		\
	$(sources)		\
	gnome-sharp.pc.in

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

gtk-sharp.pub: $(top_srcdir)/gtk-sharp.pub
	cp $(top_srcdir)/gtk-sharp.pub .

$(ASSEMBLY): $(build_sources) generated-stamp gtk-sharp.pub
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

install-data-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /i $(ASSEMBLY) /f /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

uninstall-local:
	@if test -n '$(TARGET)'; then						\
	  echo "$(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir)";	\
	  $(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE) /root $(DESTDIR)$(libdir) || exit 1;	\
	fi

