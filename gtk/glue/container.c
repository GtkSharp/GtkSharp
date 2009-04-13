/* container.c : Glue for GtkContainer
 *
 * Author:  Mike Kestner (mkestner@ximian.com)
 *
 * Copyright (C) 2004 Novell, Inc.
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

#include <gtk/gtkcontainer.h>

void gtksharp_container_base_forall (GtkContainer *container, gboolean include_internals, GtkCallback cb, gpointer data);

void 
gtksharp_container_base_forall (GtkContainer *container, gboolean include_internals, GtkCallback cb, gpointer data)
{
	GtkContainerClass *parent = g_type_class_peek_parent (G_OBJECT_GET_CLASS (container));
	if (parent->forall)
		(*parent->forall) (container, include_internals, cb, data);
}

void gtksharp_container_override_forall (GType gtype, gpointer cb);

void 
gtksharp_container_override_forall (GType gtype, gpointer cb)
{
	GtkContainerClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((GtkContainerClass *) klass)->forall = cb;
}

void gtksharp_container_invoke_gtk_callback (GtkCallback cb, GtkWidget *widget, gpointer data);

void 
gtksharp_container_invoke_gtk_callback (GtkCallback cb, GtkWidget *widget, gpointer data)
{
	cb (widget, data);
}

void gtksharp_container_child_get_property (GtkContainer *container, GtkWidget *child,
					    const gchar* property, GValue *value);

void
gtksharp_container_child_get_property (GtkContainer *container, GtkWidget *child,
				       const gchar* property, GValue *value)
{
	GParamSpec *spec = gtk_container_class_find_child_property (G_OBJECT_GET_CLASS (container), property);
	g_value_init (value, spec->value_type);
	gtk_container_child_get_property (container, child, property, value);
}

