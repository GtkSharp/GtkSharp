/* event.c : Glue to access fields in GdkEvent.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */


#include <gdk/gdkevents.h>

/* Forward declarations */
GdkEventType gtksharp_gdk_event_get_event_type (GdkEvent *event);
/* */

GdkEventType
gtksharp_gdk_event_get_event_type (GdkEvent *event)
{
	return event->type;
}

