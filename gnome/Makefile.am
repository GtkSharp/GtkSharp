SUBDIRS = glue

if ENABLE_GNOME
TARGET = $(ASSEMBLY)
APIS = $(API)
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
assemblydir = $(libdir)
gapidir = $(datadir)/gapi
assembly_DATA = $(TARGET)
gapi_DATA = $(APIS)
CLEANFILES = $(ASSEMBLY) generated-stamp generated/*.cs $(API)

sources =					\
	CanvasProxy.cs				\
	GtkSharp.BoundsHandler.cs		\
	GtkSharp.DrawHandler.cs			\
	GtkSharp.PointHandler.cs		\
	GtkSharp.RenderHandler.cs		\
	GtkSharp.UpdateHandler.cs		\
	Modules.cs				\
	voidObjectAffineSVPintSignal.cs

build_sources = $(addprefix $(srcdir)/, $(sources))

customs =					\
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
	Font.custom				\
	FontFamily.custom			\
	IconList.custom				\
	IconTextItem.custom			\
	IconTheme.custom			\
	PrintDialog.custom			\
	PrintJob.custom				\
	Program.custom

build_customs = $(addprefix $(srcdir)/, $(customs))

EXTRA_DIST =			\
	$(RAW_API)		\
	$(METADATA)		\
	$(customs)		\
	$(sources)

$(API): $(srcdir)/$(RAW_API) $(srcdir)/$(METADATA)
	cp $(srcdir)/$(RAW_API) $(API)
	chmod u+w $(API)
	$(RUNTIME) ../parser/gapi-fixup.exe --api=$(API) --metadata=$(srcdir)/$(METADATA)

generated-stamp: $(API) ../generator/gapi_codegen.exe $(build_customs)
	$(RUNTIME) ../generator/gapi_codegen.exe --generate $(API) \
	--include $(INCLUDE_API) --outdir=generated --customdir=$(srcdir) --assembly-name=$(ASSEMBLY_NAME) \
	&& touch generated-stamp

$(ASSEMBLY): $(build_sources) generated-stamp
	$(CSC) --unsafe /out:$(ASSEMBLY) /target:library $(references) $(build_sources) generated/*.cs

