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
guint32 gtksharp_gdk_event_button_get_time (GdkEventButton *event);
guint gtksharp_gdk_event_button_get_state (GdkEventButton *event);
guint gtksharp_gdk_event_button_get_button (GdkEventButton *event);
gdouble gtksharp_gdk_event_button_get_x (GdkEventButton *event);
gdouble gtksharp_gdk_event_button_get_y (GdkEventButton *event);
gdouble gtksharp_gdk_event_button_get_x_root (GdkEventButton *event);
gdouble gtksharp_gdk_event_button_get_y_root (GdkEventButton *event);
gdouble* gtksharp_gdk_event_button_get_axes (GdkEventButton *event);
GdkDevice* gtksharp_gdk_event_button_get_device (GdkEventButton *event);
guint32 gtksharp_gdk_event_scroll_get_time (GdkEventScroll *event);
guint gtksharp_gdk_event_scroll_get_state (GdkEventScroll *event);
guint gtksharp_gdk_event_scroll_get_scroll_direction (GdkEventScroll *event);
gdouble gtksharp_gdk_event_scroll_get_x (GdkEventScroll *event);
gdouble gtksharp_gdk_event_scroll_get_y (GdkEventScroll *event);
gdouble gtksharp_gdk_event_scroll_get_x_root (GdkEventScroll *event);
gdouble gtksharp_gdk_event_scroll_get_y_root (GdkEventScroll *event);
GdkDevice* gtksharp_gdk_event_scroll_get_device (GdkEventScroll *event);
guint32 gtksharp_gdk_event_motion_get_time (GdkEventMotion *event);
guint gtksharp_gdk_event_motion_get_state (GdkEventMotion *event);
guint16 gtksharp_gdk_event_motion_get_is_hint (GdkEventMotion *event);
gdouble gtksharp_gdk_event_motion_get_x (GdkEventMotion *event);
gdouble gtksharp_gdk_event_motion_get_y (GdkEventMotion *event);
gdouble gtksharp_gdk_event_motion_get_x_root (GdkEventMotion *event);
gdouble gtksharp_gdk_event_motion_get_y_root (GdkEventMotion *event);
gdouble* gtksharp_gdk_event_motion_get_axes (GdkEventMotion *event);
GdkDevice* gtksharp_gdk_event_motion_get_device (GdkEventMotion *event);
GdkRectangle gtksharp_gdk_event_expose_get_area (GdkEventExpose *event);
gint gtksharp_gdk_event_expose_get_count (GdkEventExpose *event);
GdkRegion* gtksharp_gdk_event_expose_get_region (GdkEventExpose *event);
GdkVisibilityState gtksharp_gdk_event_visibility_get_state (GdkEventVisibility *event);
guint32 gtksharp_gdk_event_crossing_get_time (GdkEventCrossing *event);
guint gtksharp_gdk_event_crossing_get_state (GdkEventCrossing *event);
gboolean gtksharp_gdk_event_crossing_get_focus (GdkEventCrossing *event);
gdouble gtksharp_gdk_event_crossing_get_x (GdkEventCrossing *event);
gdouble gtksharp_gdk_event_crossing_get_y (GdkEventCrossing *event);
gdouble gtksharp_gdk_event_crossing_get_x_root (GdkEventCrossing *event);
gdouble gtksharp_gdk_event_crossing_get_y_root (GdkEventCrossing *event);
GdkNotifyType gtksharp_gdk_event_crossing_get_detail (GdkEventCrossing *event);
GdkCrossingMode gtksharp_gdk_event_crossing_get_mode (GdkEventCrossing *event);
GdkWindow* gtksharp_gdk_event_crossing_get_subwindow (GdkEventCrossing *event);
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
	return event->state;
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

guint32
gtksharp_gdk_event_button_get_time (GdkEventButton *event)
{
	return event->time;
}

guint
gtksharp_gdk_event_button_get_state (GdkEventButton *event)
{
	return event->state;
}

guint
gtksharp_gdk_event_button_get_button (GdkEventButton *event)
{
	return event->button;
}

GdkDevice*
gtksharp_gdk_event_button_get_device (GdkEventButton *event)
{
	return event->device;
}

gdouble
gtksharp_gdk_event_button_get_x (GdkEventButton *event)
{
	return event->x;
}

gdouble
gtksharp_gdk_event_button_get_y (GdkEventButton *event)
{
	return event->y;
}

gdouble
gtksharp_gdk_event_button_get_x_root (GdkEventButton *event)
{
	return event->x_root;
}

gdouble
gtksharp_gdk_event_button_get_y_root (GdkEventButton *event)
{
	return event->y_root;
}

gdouble*
gtksharp_gdk_event_button_get_axes (GdkEventButton *event)
{
	return event->axes;
}

guint32
gtksharp_gdk_event_scroll_get_time (GdkEventScroll *event)
{
	return event->time;
}

guint
gtksharp_gdk_event_scroll_get_state (GdkEventScroll *event)
{
	return event->state;
}

GdkScrollDirection
gtksharp_gdk_event_scroll_get_direction (GdkEventScroll *event)
{
	return event->direction;
}

GdkDevice*
gtksharp_gdk_event_scroll_get_device (GdkEventScroll *event)
{
	return event->device;
}

gdouble
gtksharp_gdk_event_scroll_get_x (GdkEventScroll *event)
{
	return event->x;
}

gdouble
gtksharp_gdk_event_scroll_get_y (GdkEventScroll *event)
{
	return event->y;
}

gdouble
gtksharp_gdk_event_scroll_get_x_root (GdkEventScroll *event)
{
	return event->x_root;
}

gdouble
gtksharp_gdk_event_scroll_get_y_root (GdkEventScroll *event)
{
	return event->y_root;
}

guint32
gtksharp_gdk_event_motion_get_time (GdkEventMotion *event)
{
	return event->time;
}

guint
gtksharp_gdk_event_motion_get_state (GdkEventMotion *event)
{
	return event->state;
}

guint16
gtksharp_gdk_event_motion_get_is_hint (GdkEventMotion *event)
{
	return event->is_hint;
}

GdkDevice*
gtksharp_gdk_event_motion_get_device (GdkEventMotion *event)
{
	return event->device;
}

gdouble
gtksharp_gdk_event_motion_get_x (GdkEventMotion *event)
{
	return event->x;
}

gdouble
gtksharp_gdk_event_motion_get_y (GdkEventMotion *event)
{
	return event->y;
}

gdouble
gtksharp_gdk_event_motion_get_x_root (GdkEventMotion *event)
{
	return event->x_root;
}

gdouble
gtksharp_gdk_event_motion_get_y_root (GdkEventMotion *event)
{
	return event->y_root;
}

gdouble*
gtksharp_gdk_event_motion_get_axes (GdkEventMotion *event)
{
	return event->axes;
}

GdkRectangle
gtksharp_gdk_event_expose_get_area (GdkEventExpose *event)
{
	return event->area;
}

gint
gtksharp_gdk_event_expose_get_count (GdkEventExpose *event)
{
	return event->count;
}

GdkRegion*
gtksharp_gdk_event_expose_get_region (GdkEventExpose *event)
{
	return event->region;
}

GdkVisibilityState 
gtksharp_gdk_event_visibility_get_state (GdkEventVisibility *event)
{
	return event->state;
}

gdouble
gtksharp_gdk_event_crossing_get_x (GdkEventCrossing *event)
{
	return event->x;
}

gdouble
gtksharp_gdk_event_crossing_get_y (GdkEventCrossing *event)
{
	return event->y;
}

gdouble
gtksharp_gdk_event_crossing_get_x_root (GdkEventCrossing *event)
{
	return event->x_root;
}

gdouble
gtksharp_gdk_event_crossing_get_y_root (GdkEventCrossing *event)
{
	return event->y_root;
}

guint32
gtksharp_gdk_event_crossing_get_time (GdkEventCrossing *event)
{
	return event->time;
}

guint
gtksharp_gdk_event_crossing_get_state (GdkEventCrossing *event)
{
	return event->state;
}

gboolean
gtksharp_gdk_event_crossing_get_focus (GdkEventCrossing *event)
{
	return event->focus;
}

GdkWindow*
gtksharp_gdk_event_crossing_get_subwindow (GdkEventCrossing *event)
{
	return event->subwindow;
}

GdkCrossingMode
gtksharp_gdk_event_crossing_get_mode (GdkEventCrossing *event)
{
	return event->mode;
}

GdkNotifyType
gtksharp_gdk_event_crossing_get_detail (GdkEventCrossing *event)
{
	return event->detail;
}

