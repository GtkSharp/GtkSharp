SUBDIRS = . glue

if ENABLE_MONO_CAIRO
local_mono_cairo=$(top_builddir)/cairo/Mono.Cairo.dll
else
local_mono_cairo=
endif

pkg = gdk
SYMBOLS = gdk-symbols.xml
INCLUDE_API = $(srcdir)/../glib/glib-api.xml ../pango/pango-api.xml
METADATA = Gdk.metadata
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll $(local_mono_cairo)
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

include ../Makefile.include
