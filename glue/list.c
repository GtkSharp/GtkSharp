/* list.c : Glue to access fields in GList.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */


#include <glib/glist.h>

gpointer 
gtksharp_list_get_data (GList *l)
{
	return l->data;
}

GList* 
gtksharp_list_get_next (GList *l)
{
	return l->next;
}
