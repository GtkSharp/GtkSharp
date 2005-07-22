/* type.c : GType utilities
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * Copyright (c) 2002 Mike Kestner
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

#include <glib-object.h>
#include <stdio.h>

/* Forward declarations */
G_CONST_RETURN gchar   *gtksharp_get_type_name (GObject *obj);

gboolean gtksharp_is_object (gpointer obj);

GType    gtksharp_get_type_id (GObject *obj);

GType    gtksharp_register_type (gchar *name, GType parent);

void     gtksharp_override_virtual_method (GType g_type, const gchar *name, GCallback callback);
/* */

G_CONST_RETURN gchar *
gtksharp_get_type_name (GObject *obj)
{
	return G_OBJECT_TYPE_NAME (obj);
}

gboolean
gtksharp_is_object (gpointer obj)
{
	return G_IS_OBJECT (obj);
}

GType
gtksharp_get_type_id (GObject *obj)
{
	return G_TYPE_FROM_INSTANCE (obj);
}

GType
gtksharp_register_type (gchar *name, GType parent)
{
	GTypeQuery query;
	GTypeInfo info = {0, NULL, NULL, NULL, NULL, NULL, 0, 0, NULL, NULL };

	g_type_query (parent, &query);

	info.class_size = query.class_size;
	info.instance_size = query.instance_size;

	return g_type_register_static (parent, name, &info, 0);
}

void
gtksharp_override_virtual_method (GType g_type, const gchar *name, GCallback callback)
{
	guint id;
	GClosure *closure;
	if (g_type_class_peek (g_type) == NULL)
		g_type_class_ref (g_type);
	id = g_signal_lookup (name, g_type);
	closure = g_cclosure_new (callback, NULL, NULL);
	g_signal_override_class_closure (id, g_type, closure);
}
