/* dragcontext.c: Glue for accessing fields in a GdkDragContext.
 *
 * Author: Ettore Perazzoli <ettore@ximian.com>
 *
 * Copyright (c) 2003 Novell, Inc.
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
