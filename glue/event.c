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

