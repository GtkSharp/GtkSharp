/* object.c : Glue to clean up GtkObject references.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>
#include <gtk/gtkobject.h>

/* Forward declarations */
void     gtksharp_object_unref_if_floating (GObject *obj);
gboolean gtksharp_object_is_floating (GObject *obj);
/* */

void
gtksharp_object_unref_if_floating (GObject *obj)
{
	if (GTK_OBJECT_FLOATING (obj))
		g_object_unref (obj);
}

gboolean
gtksharp_object_is_floating (GObject *obj)
{
	return GTK_OBJECT_FLOATING (obj);
}

