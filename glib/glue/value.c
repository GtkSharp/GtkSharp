/* value.c : Glue to allocate GValues on the heap.
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

/* Forward declarations */
void gtksharp_value_create_from_property (GValue *value, GObject *obj, const gchar* name);
void gtksharp_value_create_from_type_and_property (GValue *value, GType gtype, const gchar* name);
void gtksharp_value_create_from_type_name (GValue *value, const gchar *type_name);
GType gtksharp_value_get_value_type (GValue *value);
gpointer glibsharp_value_get_boxed (GValue *value);
void glibsharp_value_set_boxed (GValue *value, gpointer boxed);
gboolean glibsharp_value_holds_flags (GValue *value);
/* */

void
gtksharp_value_create_from_property (GValue *value, GObject *obj, const gchar* name)
{
	GParamSpec *spec = g_object_class_find_property (G_OBJECT_GET_CLASS (obj), name);
	g_value_init (value, spec->value_type);
}

void
gtksharp_value_create_from_type_and_property (GValue *value, GType gtype, const gchar* name)
{
	GParamSpec *spec = g_object_class_find_property (g_type_class_ref (gtype), name);
	g_value_init (value, spec->value_type);
}

void
gtksharp_value_create_from_type_name (GValue *value, const gchar *type_name)
{
	g_value_init (value, g_type_from_name (type_name));
}

GType
gtksharp_value_get_value_type (GValue *value) 
{
	g_return_val_if_fail (value != NULL, G_TYPE_INVALID);
	g_return_val_if_fail (G_IS_VALUE (value), G_TYPE_INVALID);
	return G_VALUE_TYPE (value);
}

gpointer 
glibsharp_value_get_boxed (GValue *value)
{
	return g_value_get_boxed (value);
}

void 
glibsharp_value_set_boxed (GValue *value, gpointer boxed)
{
	g_value_set_boxed (value, boxed);
}

gboolean
glibsharp_value_holds_flags (GValue *value)
{
	return G_VALUE_HOLDS_FLAGS (value);
}
