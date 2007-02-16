/* cellrenderer.c : Glue for overriding pieces of GtkCellRenderer
 *
 * Author: Todd Berman (tberman@sevenl.net),
 *         Peter Johanson (peter@peterjohanson.com)
 * 
 * Copyright (C) 2004 Todd Berman
 * Copyright (C) 2007 Peter Johanson
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

#include <gtk/gtkcellrenderer.h>

void gtksharp_cellrenderer_invoke_get_size (GType gtype, GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height);

void
gtksharp_cellrenderer_invoke_get_size (GType type, GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height)
{
	GtkCellRendererClass *klass = g_type_class_peek (type);
	klass->get_size (cell, widget, cell_area, x_offset, y_offset, width, height);
}

void gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height);

void
gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height)
{
	if (GTK_CELL_RENDERER_GET_CLASS (cell)->get_size)
		GTK_CELL_RENDERER_GET_CLASS (cell)->get_size (cell, widget, cell_area, x_offset, y_offset, width, height);
}

void gtksharp_cellrenderer_override_get_size (GType gtype, gpointer cb);

void
gtksharp_cellrenderer_override_get_size (GType gtype, gpointer cb)
{
	GtkCellRendererClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((GtkCellRendererClass *) klass)->get_size = cb;
}

void gtksharp_cellrenderer_invoke_render (GType type, GtkCellRenderer *cell, GdkDrawable *window, GtkWidget *widget, GdkRectangle *background_area, GdkRectangle *cell_area, GdkRectangle *expose_area, GtkCellRendererState flags);

void
gtksharp_cellrenderer_invoke_render (GType type, GtkCellRenderer *cell, GdkDrawable *window, GtkWidget *widget, GdkRectangle *background_area, GdkRectangle *cell_area, GdkRectangle *expose_area, GtkCellRendererState flags)
{
	GtkCellRendererClass *klass = g_type_class_peek (type);
	klass->render (cell, window, widget, background_area, cell_area, expose_area, flags);
}

void gtksharp_cellrenderer_base_render (GtkCellRenderer *cell, GdkDrawable *window, GtkWidget *widget, GdkRectangle *background_area, GdkRectangle *cell_area, GdkRectangle *expose_area, GtkCellRendererState flags);

void
gtksharp_cellrenderer_base_render (GtkCellRenderer *cell, GdkDrawable *window, GtkWidget *widget, GdkRectangle *background_area, GdkRectangle *cell_area, GdkRectangle *expose_area, GtkCellRendererState flags)
{
	if (GTK_CELL_RENDERER_GET_CLASS (cell)->render)
		GTK_CELL_RENDERER_GET_CLASS (cell)->render (cell, window, widget, background_area, cell_area, expose_area, flags);
}

void gtksharp_cellrenderer_override_render (GType gtype, gpointer cb);

void
gtksharp_cellrenderer_override_render (GType gtype, gpointer cb)
{
	GtkCellRendererClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((GtkCellRendererClass *) klass)->render = cb;
}

GtkCellEditable * gtksharp_cellrenderer_invoke_start_editing (GType type, GtkCellRenderer *cell, GdkEvent *event, GtkWidget *widget, const gchar *path, GdkRectangle *background_area, GdkRectangle *cell_area, GtkCellRendererState flags);

GtkCellEditable *
gtksharp_cellrenderer_invoke_start_editing (GType type, GtkCellRenderer *cell, GdkEvent *event, GtkWidget *widget, const gchar *path, GdkRectangle *background_area, GdkRectangle *cell_area, GtkCellRendererState flags)
{
	GtkCellRendererClass *klass = g_type_class_peek (type);
	klass->start_editing (cell, event, widget, path, background_area, cell_area, flags);
}

GtkCellEditable * gtksharp_cellrenderer_base_start_editing (GtkCellRenderer *cell, GdkEvent *event, GtkWidget *widget, const gchar *path, GdkRectangle *background_area, GdkRectangle *cell_area, GtkCellRendererState flags);

GtkCellEditable *
gtksharp_cellrenderer_base_start_editing (GtkCellRenderer *cell, GdkEvent *event, GtkWidget *widget, const gchar *path, GdkRectangle *background_area, GdkRectangle *cell_area, GtkCellRendererState flags)
{
	if (GTK_CELL_RENDERER_GET_CLASS (cell)->start_editing)
		return GTK_CELL_RENDERER_GET_CLASS (cell)->start_editing (cell, event, widget, path, background_area, cell_area, flags);
	return NULL;	
}


void gtksharp_cellrenderer_override_start_editing (GType gtype, gpointer cb);

void
gtksharp_cellrenderer_override_start_editing (GType gtype, gpointer cb)
{
	GtkCellRendererClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((GtkCellRendererClass *) klass)->start_editing = cb;
}
