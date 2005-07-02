SUBDIRS = . glue

pkg = gdk
SYMBOLS = gdk-symbols.xml
INCLUDE_API = ../pango/pango-api.xml
METADATA = Gdk.metadata
references = ../glib/glib-sharp.dll ../pango/pango-sharp.dll
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
	PangoAttrStipple.custom \
	Pixmap.custom		\
	Pixbuf.custom		\
	PixbufAnimation.custom	\
	PixbufFrame.custom	\
	PixbufLoader.custom	\
	Pixdata.custom		\
	Point.custom		\
	Rectangle.custom	\
	Region.custom		\
	Screen.custom		\
	Selection.custom	\
	WindowAttr.custom	\
	Window.custom

add_dist = makefile.win32

include ../Makefile.include
