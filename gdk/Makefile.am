SUBDIRS = . glue

pkg = gdk
SYMBOLS = gdk-symbols.xml
INCLUDE_API = $(top_srcdir)/glib/glib-api.xml $(top_srcdir)/cairo/cairo-api.xml $(top_builddir)/gio/gio-api.xml $(top_builddir)/pango/pango-api.xml
METADATA = Gdk.metadata
references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/gio/gio-sharp.dll $(top_builddir)/pango/pango-sharp.dll $(top_builddir)/cairo/cairo-sharp.dll
glue_includes = gdk/gdk.h

sources = 			\
	EventButton.cs		\
	EventClient.cs		\
	EventConfigure.cs	\
	EventCrossing.cs	\
	Event.cs		\
	EventDND.cs		\
	EventExpose.cs		\
	EventFocus.cs		\
	EventGrabBroken.cs	\
	EventKey.cs		\
	EventMotion.cs		\
	EventOwnerChange.cs	\
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

customs = 			\
	Atom.custom		\
	Color.custom		\
	Colormap.custom		\
	Device.custom		\
	DeviceAxis.custom	\
	Display.custom		\
	DisplayManager.custom	\
	DragContext.custom	\
	Drawable.custom		\
	EdgeTable.custom	\
	GCValues.custom		\
	Global.custom		\
	Input.custom		\
	Keymap.custom		\
	PangoAttrEmbossed.custom\
	PangoAttrEmbossColor.custom	\
	PangoAttrStipple.custom \
	Pixmap.custom		\
	Pixbuf.custom		\
	PixbufAnimation.custom	\
	PixbufFormat.custom	\
	PixbufFrame.custom	\
	PixbufLoader.custom	\
	Pixdata.custom		\
	Point.custom		\
	Property.custom		\
	Rectangle.custom	\
	Region.custom		\
	RgbCmap.custom		\
	Screen.custom		\
	Selection.custom	\
	WindowAttr.custom	\
	Window.custom

add_dist = 

include $(top_srcdir)/Makefile.include
