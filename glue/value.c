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
	val = g_value_init (val, g_type);
	return val;
}


