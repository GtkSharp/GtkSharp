/* valuearray.c : Glue to access GValueArray fields
 *
 * Author: Mike Kestner <mkestner@ximian.com>
 *
 * <c> 2004 Novell, Inc.
 */

#include <glib-object.h>

GValue *gtksharp_value_array_get_array (GValueArray *va);
guint gtksharp_value_array_get_count (GValueArray *va);

GValue *
gtksharp_value_array_get_array (GValueArray *va)
{
	return va->values;
}

guint
gtksharp_value_array_get_count (GValueArray *va)
{
	return va->n_values;
}

