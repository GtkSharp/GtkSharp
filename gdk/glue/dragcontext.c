/* dragcontext.c: Glue for accessing fields in a GdkDragContext.
 *
 * Author: Ettore Perazzoli <ettore@ximian.com>
 *
 * (C) 2003 Novell, Inc.
 */

#include <gdk/gdkdnd.h>

GdkDragProtocol gtksharp_drag_context_get_protocol (GdkDragContext *context);

GdkDragProtocol
gtksharp_drag_context_get_protocol (GdkDragContext *context)
{
	return context->protocol;
}


gboolean gtksharp_drag_context_get_is_source (GdkDragContext *context);

gboolean
gtksharp_drag_context_get_is_source (GdkDragContext *context)
{
	return context->is_source;
}


GdkWindow *
gtksharp_drag_context_get_source_window (GdkDragContext *context)
{
	return context->source_window;
}


GdkWindow *gtksharp_drag_context_get_dest_window (GdkDragContext *context);

GdkWindow *
gtksharp_drag_context_get_dest_window (GdkDragContext *context)
{
	return context->dest_window;
}


GList *gtksharp_drag_context_get_targets (GdkDragContext *context);

GList *
gtksharp_drag_context_get_targets (GdkDragContext *context)
{
	return context->targets;
}


GdkDragAction gtksharp_drag_context_get_actions (GdkDragContext *context);

GdkDragAction
gtksharp_drag_context_get_actions (GdkDragContext *context)
{
	return context->actions;
}


GdkDragAction gtksharp_drag_context_get_suggested_action (GdkDragContext *context);

GdkDragAction
gtksharp_drag_context_get_suggested_action (GdkDragContext *context)
{
	return context->suggested_action;
}


GdkDragAction gtksharp_drag_context_get_action (GdkDragContext *context);

GdkDragAction
gtksharp_drag_context_get_action (GdkDragContext *context)
{
	return context->action;
}


guint32 gtksharp_drag_context_get_start_time (GdkDragContext *context);

guint32
gtksharp_drag_context_get_start_time (GdkDragContext *context)
{
	return context->start_time;
}
