/* event.c : Glue to access fields in GdkEvent.
 *
 * Authors: Rachel Hestilow  <hestilow@ximian.com>
 *          Mike Kestner  <mkestner@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 * (c) 2004 Novell, Inc.
 */


#include <gdk/gdkevents.h>

/* Forward declarations */
GdkEventType gtksharp_gdk_event_get_event_type (GdkEvent *event);
GdkWindow* gtksharp_gdk_event_get_window (GdkEventAny *event);
gint8 gtksharp_gdk_event_get_send_event (GdkEventAny *event);
guint32 gtksharp_gdk_event_key_get_time (GdkEventKey *event);
guint gtksharp_gdk_event_key_get_state (GdkEventKey *event);
guint gtksharp_gdk_event_key_get_keyval (GdkEventKey *event);
guint16 gtksharp_gdk_event_key_get_hardware_keycode (GdkEventKey *event);
guint8 gtksharp_gdk_event_key_get_group (GdkEventKey *event);
/* */

GdkEventType
gtksharp_gdk_event_get_event_type (GdkEvent *event)
{
	return event->type;
}

GdkWindow*
gtksharp_gdk_event_get_window (GdkEventAny *event)
{
	return event->window;
}

gint8
gtksharp_gdk_event_get_send_event (GdkEventAny *event)
{
	return event->send_event;
}

guint32
gtksharp_gdk_event_key_get_time (GdkEventKey *event)
{
	return event->time;
}

guint
gtksharp_gdk_event_key_get_state (GdkEventKey *event)
{
	return event->time;
}

guint
gtksharp_gdk_event_key_get_keyval (GdkEventKey *event)
{
	return event->keyval;
}

guint16
gtksharp_gdk_event_key_get_hardware_keycode (GdkEventKey *event)
{
	return event->hardware_keycode;
}

guint8
gtksharp_gdk_event_key_get_group (GdkEventKey *event)
{
	return event->group;
}

