/* textiter.c : Glue to allocate GtkTextIters on the heap.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */

#include <gtk/gtktextiter.h>

GtkTextIter*
gtksharp_text_iter_create (void)
{
	GtkTextIter *iter = g_new0 (GtkTextIter, 1);
	return iter;
}

void
gtksharp_test_array (int len, int* types)
{
	int i;
	for (i = 0; i < len; i++)
		g_print ("type %i\n", types[i]);
}
