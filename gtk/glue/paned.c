/* paned.c : Glue for accessing fields in the GtkPaned widget.
 *
 * Author: Duncan Mak  (duncan@ximian.com)
 *
 * (C) Ximian, INc.
 */

#include <gtk/gtkpaned.h>

/* Forward declarations */
GtkWidget *gtksharp_paned_get_child1 (GtkPaned *paned);
GtkWidget *gtksharp_paned_get_child2 (GtkPaned *paned);
/* */

GtkWidget*
gtksharp_paned_get_child1 (GtkPaned *paned)
{
	return paned->child1;
}

GtkWidget*
gtksharp_paned_get_child2 (GtkPaned *paned)
{
	return paned->child2;
}
