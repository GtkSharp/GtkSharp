/* value.c : Glue to allocate GValues on the heap.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>

GValue *
gtksharp_value_create (GType g_type)
{
	GValue *val = g_new0 (GValue, 1);
	if (g_type != G_TYPE_INVALID)
		val = g_value_init (val, g_type);
	return val;
}

GValue *
gtksharp_value_create_from_property (GObject *obj, const gchar* name)
{
	GParamSpec *spec = g_object_class_find_property (
				G_OBJECT_GET_CLASS (obj), name);
	return gtksharp_value_create (spec->value_type);
}

