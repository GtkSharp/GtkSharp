SUBDIRS = . glue

if ENABLE_GNOME
pkg = gnome
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = gnome-sharp-2.0.pc
else
pkg = 
endif

INCLUDE_API = ../pango/pango-api.xml ../atk/atk-api.xml ../gdk/gdk-api.xml ../gtk/gtk-api.xml ../art/art-api.xml ../gnomevfs/gnome-vfs-api.xml
METADATA = Gnome.metadata
SYMBOLS =
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll ../atk/atk-sharp.dll ../gdk/gdk-sharp.dll ../gtk/gtk-sharp.dll ../art/art-sharp.dll ../gnomevfs/gnome-vfs-sharp.dll
glue_includes = gnome.h,libgnomeprint/gnome-print.h,libgnomeprint/gnome-print-paper.h

sources =					\
	BonoboUIVerb.cs				\
	FontFamily.cs				\
	IconTheme.cs				\
	Modules.cs				\
	PanelAppletFactory.cs

customs =					\
	About.custom				\
	App.custom				\
	DateEdit.custom				\
	CanvasBpath.custom			\
	CanvasClipgroup.custom			\
	Canvas.custom				\
	CanvasEllipse.custom			\
	CanvasGroup.custom			\
	CanvasItem.custom			\
	CanvasLine.custom			\
	CanvasPathDef.custom			\
	CanvasPixbuf.custom			\
	CanvasPoints.custom			\
	CanvasPolygon.custom			\
	CanvasRect.custom			\
	CanvasRE.custom				\
	CanvasRichText.custom			\
	CanvasShape.custom			\
	CanvasText.custom			\
	CanvasWidget.custom			\
	Client.custom				\
	Druid.custom				\
	DruidPageEdge.custom			\
	Font.custom				\
	IconList.custom				\
	IconTextItem.custom			\
	PanelApplet.custom			\
	Popup.custom				\
	Print.custom				\
	PrintContext.custom			\
	PrintDialog.custom			\
	PrintJob.custom				\
	Program.custom				\
	Scores.custom				\
	UIInfo.custom

add_dist = gnome-sharp-2.0.pc.in

include ../Makefile.include

