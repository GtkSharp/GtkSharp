/* slist.c : Glue to access fields in GSList.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */


#include <glib/gslist.h>

gpointer 
gtksharp_slist_get_data (GSList *l)
{
	return l->data;
}

GSList* 
gtksharp_slist_get_next (GSList *l)
{
	return l->next;
}
