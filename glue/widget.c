/* widget.c : Glue to access fields in GtkWidget.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */

#include <gtk/gtkwidget.h>

/* Forward declarations */
GdkRectangle *gtksharp_gtk_widget_get_allocation (GtkWidget *widget);
GdkWindow *gtksharp_gtk_widget_get_window (GtkWidget *widget);
int gtksharp_gtk_widget_get_state (GtkWidget *widget);
/* */

GdkRectangle*
gtksharp_gtk_widget_get_allocation (GtkWidget *widget)
{
	return &widget->allocation;
}

GdkWindow *
gtksharp_gtk_widget_get_window (GtkWidget *widget)
{
	return widget->window;
}

int
gtksharp_gtk_widget_get_state (GtkWidget *widget)
{
	return GTK_WIDGET_STATE (widget);
}
