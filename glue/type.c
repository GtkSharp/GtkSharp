/* value.c : Glue to allocate GValues on the heap.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>

gchar *
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
gtksharp_get_parent_type (GType typ)
{
	return g_type_parent (typ);
}

gchar *
gtksharp_get_type_name_for_id (GType typ)
{
	return g_type_name (typ);
}
