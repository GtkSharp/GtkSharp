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


