/* cellrenderer.c : Glue for overriding pieces of GtkCellRenderer
 *
 * Author: Todd Berman (tberman@sevenl.net)
 * 
 * Copyright (C) 2004 Todd Berman
 */

#include <gtk/gtkcellrenderer.h>

void gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height);

void
gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height)
{
	GtkCellRendererClass *parent = g_type_class_peek_parent (G_OBJECT_GET_CLASS (cell));
	if (parent->get_size)
		(*parent->get_size) (cell, widget, cell_area, x_offset, y_offset, width, height);
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

void gtksharp_cellrenderer_base_render (GtkCellRenderer *cell, GdkDrawable *window, GtkWidget *widget, GdkRectangle *background_area, GdkRectangle *cell_area, GdkRectangle *expose_area, GtkCellRendererState flags);

void
gtksharp_cellrenderer_base_render (GtkCellRenderer *cell, GdkDrawable *window, GtkWidget *widget, GdkRectangle *background_area, GdkRectangle *cell_area, GdkRectangle *expose_area, GtkCellRendererState flags)
{
	GtkCellRendererClass *parent = g_type_class_peek_parent (G_OBJECT_GET_CLASS (cell));
	if (parent->render)
		(*parent->render) (cell, window, widget, background_area, cell_area, expose_area, flags);
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
