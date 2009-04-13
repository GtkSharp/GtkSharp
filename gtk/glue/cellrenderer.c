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

const gchar *__gtype_prefix = "__gtksharp_";
#define HAS_PREFIX(a) (*((guint64 *)(a)) == *((guint64 *) __gtype_prefix))

static GObjectClass *
get_threshold_class (GObject *obj)
{
	GType gtype = G_TYPE_FROM_INSTANCE (obj);
	while (HAS_PREFIX (g_type_name (gtype)))
		gtype = g_type_parent (gtype);
	GObjectClass *klass = g_type_class_peek (gtype);
	if (klass == NULL) klass = g_type_class_ref (gtype);
	return klass;
}

void gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height);

void
gtksharp_cellrenderer_base_get_size (GtkCellRenderer *cell, GtkWidget *widget, GdkRectangle *cell_area, gint *x_offset, gint *y_offset, gint *width, gint *height)
{
	GtkCellRendererClass *klass = (GtkCellRendererClass *) get_threshold_class (G_OBJECT (cell));
	if (klass->get_size)
		(* klass->get_size) (cell, widget, cell_area, x_offset, y_offset, width, height);
}
void gtksharp_cellrenderer_override_get_size (GType gtype, gpointer cb);

void
gtksharp_cellrenderer_override_get_size (GType gtype, gpointer cb)
{
	GObjectClass *klass = g_type_class_peek (gtype);
	if (klass == NULL)
		klass = g_type_class_ref (gtype);
	((GtkCellRendererClass *) klass)->get_size = cb;
}
