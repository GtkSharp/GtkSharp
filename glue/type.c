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

