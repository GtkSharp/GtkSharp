/* container.c : Glue for GtkContainer
 *
 * Author:  Mike Kestner (mkestner@ximian.com)
 *
 * Copyright (C) 2004 Novell, Inc.
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
