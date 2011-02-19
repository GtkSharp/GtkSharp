pkg = gdk
SYMBOLS = gdk-symbols.xml
INCLUDE_API = $(top_srcdir)/glib/glib-api.xml $(top_srcdir)/cairo/cairo-api.xml $(top_builddir)/gio/gio-api.xml $(top_builddir)/pango/pango-api.xml
METADATA = Gdk.metadata
references = $(top_builddir)/glib/glib-sharp.dll $(top_builddir)/gio/gio-sharp.dll $(top_builddir)/pango/pango-sharp.dll $(top_builddir)/cairo/cairo-sharp.dll
glue_includes = gdk/gdk.h

sources = 			\
	EventButton.cs		\
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
	Rectangle.cs		\
	Size.cs			\
	TextProperty.cs

customs = 			\
	Atom.custom		\
	Color.custom		\
	Device.custom		\
	Display.custom		\
	DisplayManager.custom	\
	Global.custom		\
	Keymap.custom		\
	Pixbuf.custom		\
	PixbufAnimation.custom	\
	PixbufFrame.custom	\
	PixbufLoader.custom	\
	Pixdata.custom		\
	Point.custom		\
	Property.custom		\
	Screen.custom		\
	Selection.custom	\
	WindowAttr.custom	\
	Window.custom

add_dist = 

include $(top_srcdir)/Makefile.include
