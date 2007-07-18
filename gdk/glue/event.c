/* event.c : Glue to access fields in GdkEvent.
 *
 * Authors: Rachel Hestilow  <hestilow@ximian.com>
 *          Mike Kestner  <mkestner@ximian.com>
 *
 * Copyright (c) 2002 Rachel Hestilow
 * Copyright (c) 2002 Mike Kestner
 * Copyright (c) 2004 Novell, Inc.
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of version 2 of the Lesser GNU General 
 * Public License as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
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
guint gtksharp_gdk_event_scroll_get_direction (GdkEventScroll *event);
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
GdkRectangle* gtksharp_gdk_event_expose_get_area (GdkEventExpose *event);
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
gint16 gtksharp_gdk_event_focus_get_in (GdkEventFocus *event);
gint gtksharp_gdk_event_configure_get_x (GdkEventConfigure *event);
gint gtksharp_gdk_event_configure_get_y (GdkEventConfigure *event);
gint gtksharp_gdk_event_configure_get_width (GdkEventConfigure *event);
gint gtksharp_gdk_event_configure_get_height (GdkEventConfigure *event);
guint32 gtksharp_gdk_event_property_get_time (GdkEventProperty *event);
GdkAtom gtksharp_gdk_event_property_get_atom (GdkEventProperty *event);
guint gtksharp_gdk_event_property_get_state (GdkEventProperty *event);
GdkNativeWindow gtksharp_gdk_event_selection_get_requestor (GdkEventSelection *event);
GdkAtom gtksharp_gdk_event_selection_get_property (GdkEventSelection *event);
GdkAtom gtksharp_gdk_event_selection_get_selection (GdkEventSelection *event);
GdkAtom gtksharp_gdk_event_selection_get_target (GdkEventSelection *event);
guint32 gtksharp_gdk_event_selection_get_time (GdkEventSelection *event);
guint32 gtksharp_gdk_event_dnd_get_time (GdkEventDND *event);
gshort gtksharp_gdk_event_dnd_get_x_root (GdkEventDND *event);
gshort gtksharp_gdk_event_dnd_get_y_root (GdkEventDND *event);
GdkDragContext* gtksharp_gdk_event_dnd_get_context (GdkEventDND *event);
GdkDevice* gtksharp_gdk_event_proximity_get_device (GdkEventProximity *event);
guint32 gtksharp_gdk_event_proximity_get_time (GdkEventProximity *event);
GdkAtom gtksharp_gdk_event_client_get_message_type (GdkEventClient *event);
gushort gtksharp_gdk_event_client_get_data_format (GdkEventClient *event);
gpointer gtksharp_gdk_event_client_get_data (GdkEventClient *event);
GdkWindowState gtksharp_gdk_event_window_state_get_changed_mask (GdkEventWindowState *event);
GdkWindowState gtksharp_gdk_event_window_state_get_new_window_state (GdkEventWindowState *event);
GdkSettingAction gtksharp_gdk_event_setting_get_action (GdkEventSetting *event);
char* gtksharp_gdk_event_setting_get_name (GdkEventSetting *event);
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

GdkRectangle*
gtksharp_gdk_event_expose_get_area (GdkEventExpose *event)
{
	return &event->area;
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

gint16
gtksharp_gdk_event_focus_get_in (GdkEventFocus *event)
{
	return event->in;
}

gint
gtksharp_gdk_event_configure_get_x (GdkEventConfigure *event)
{
	return event->x;
}

gint
gtksharp_gdk_event_configure_get_y (GdkEventConfigure *event)
{
	return event->y;
}

gint
gtksharp_gdk_event_configure_get_width (GdkEventConfigure *event)
{
	return event->width;
}

gint
gtksharp_gdk_event_configure_get_height (GdkEventConfigure *event)
{
	return event->height;
}

guint32 
gtksharp_gdk_event_property_get_time (GdkEventProperty *event)
{
	return event->time;
}

GdkAtom 
gtksharp_gdk_event_property_get_atom (GdkEventProperty *event)
{
	return event->atom;
}

guint 
gtksharp_gdk_event_property_get_state (GdkEventProperty *event)
{
	return event->state;
}

GdkNativeWindow 
gtksharp_gdk_event_selection_get_requestor (GdkEventSelection *event)
{
	return event->requestor;
}

GdkAtom 
gtksharp_gdk_event_selection_get_property (GdkEventSelection *event)
{
	return event->property;
}

GdkAtom 
gtksharp_gdk_event_selection_get_selection (GdkEventSelection *event)
{
	return event->selection;
}

GdkAtom 
gtksharp_gdk_event_selection_get_target (GdkEventSelection *event)
{
	return event->target;
}

guint32 
gtksharp_gdk_event_selection_get_time (GdkEventSelection *event)
{
	return event->time;
}

GdkDragContext* 
gtksharp_gdk_event_dnd_get_context (GdkEventDND *event)
{
	return event->context;
}

gshort 
gtksharp_gdk_event_dnd_get_x_root (GdkEventDND *event)
{
	return event->x_root;
}

gshort 
gtksharp_gdk_event_dnd_get_y_root (GdkEventDND *event)
{
	return event->y_root;
}

guint32 
gtksharp_gdk_event_dnd_get_time (GdkEventDND *event)
{
	return event->time;
}

GdkDevice* 
gtksharp_gdk_event_proximity_get_device (GdkEventProximity *event)
{
	return event->device;
}

guint32 
gtksharp_gdk_event_proximity_get_time (GdkEventProximity *event)
{
	return event->time;
}

GdkAtom 
gtksharp_gdk_event_client_get_message_type (GdkEventClient *event)
{
	return event->message_type;
}

gushort 
gtksharp_gdk_event_client_get_data_format (GdkEventClient *event)
{
	return event->data_format;
}

gpointer 
gtksharp_gdk_event_client_get_data (GdkEventClient *event)
{
	return &event->data;
}

GdkWindowState 
gtksharp_gdk_event_window_state_get_changed_mask (GdkEventWindowState *event)
{
	return event->changed_mask;
}

GdkWindowState 
gtksharp_gdk_event_window_state_get_new_window_state (GdkEventWindowState *event)
{
	return event->new_window_state;
}

GdkSettingAction 
gtksharp_gdk_event_setting_get_action (GdkEventSetting *event)
{
	return event->action;
}

char* 
gtksharp_gdk_event_setting_get_name (GdkEventSetting *event)
{
	return event->name;
}

