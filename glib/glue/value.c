/* value.c : Glue to allocate GValues on the heap.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>

/* Forward declarations */
void gtksharp_value_create_from_property (GValue *value, GObject *obj, const gchar* name);
GType gtksharp_value_get_value_type (GValue *value);
/* */

void
gtksharp_value_create_from_property (GValue *value, GObject *obj, const gchar* name)
{
	GParamSpec *spec = g_object_class_find_property (G_OBJECT_GET_CLASS (obj), name);
	g_value_init (value, spec->value_type);
}

GType
gtksharp_value_get_value_type (GValue *value) 
{
	g_return_val_if_fail (value != NULL, G_TYPE_INVALID);
	g_return_val_if_fail (G_IS_VALUE (value), G_TYPE_INVALID);
	return G_VALUE_TYPE (value);
}

