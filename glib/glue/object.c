/* object.c : Glue to clean up GtkObject references.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib-object.h>

/* Forward declarations */
int      gtksharp_object_get_ref_count (GObject *obj);
GObject *gtksharp_object_newv (GType type, gint cnt, gchar **names, GValue *vals);
/* */

int
gtksharp_object_get_ref_count (GObject *obj)
{
	return obj->ref_count;
}

GObject *
gtksharp_object_newv (GType type, gint cnt, gchar **names, GValue *vals)
{
	int i;
	GParameter *parms = NULL;
	GObject *result;

	if (cnt > 0)
		parms = g_new0 (GParameter, cnt);

	for (i = 0; i < cnt; i++) {
		parms[i].name = names[i];
		parms[i].value = vals[i];
	}

	result = g_object_newv (type, cnt, parms);

	g_free (parms);
	return result;
}

