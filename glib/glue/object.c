/* object.c : Glue to clean up GtkObject references.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>

/* Forward declarations */
int      gtksharp_object_get_ref_count (GObject *obj);
/* */

int
gtksharp_object_get_ref_count (GObject *obj)
{
	return obj->ref_count;
}
