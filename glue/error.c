/* error.c : Glue to access GError values.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * <c> 2002 Mike Kestner
 */

#include <glib.h>

gchar *
gtksharp_error_get_message (GError *err)
{
	return err->message;
}

