/* widget.c : Glue to access fields in GtkWidget.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */

#include <gtk/gtkwidget.h>

/* Forward declarations */
GdkRectangle *gtksharp_gtk_widget_get_allocation (GtkWidget *widget);
void gtksharp_gtk_widget_set_allocation (GtkWidget *widget, GdkRectangle rect);
GdkWindow *gtksharp_gtk_widget_get_window (GtkWidget *widget);
void gtksharp_gtk_widget_set_window (GtkWidget *widget, GdkWindow *window);
int gtksharp_gtk_widget_get_state (GtkWidget *widget);
int gtksharp_gtk_widget_get_flags (GtkWidget *widget);
void gtksharp_gtk_widget_set_flags (GtkWidget *widget, int flags);
int gtksharp_gtk_widget_style_get_int (GtkWidget *widget, const char *name);
/* */

GdkRectangle*
gtksharp_gtk_widget_get_allocation (GtkWidget *widget)
{
	return &widget->allocation;
}

void
gtksharp_gtk_widget_set_allocation (GtkWidget *widget, GdkRectangle rect)
{
	widget->allocation = rect;
}

GdkWindow *
gtksharp_gtk_widget_get_window (GtkWidget *widget)
{
	return widget->window;
}

void
gtksharp_gtk_widget_set_window (GtkWidget *widget, GdkWindow *window)
{
	widget->window = window;
}

int
gtksharp_gtk_widget_get_state (GtkWidget *widget)
{
	return GTK_WIDGET_STATE (widget);
}

int
gtksharp_gtk_widget_get_flags (GtkWidget *widget)
{
	return GTK_WIDGET_FLAGS (widget);
}

void
gtksharp_gtk_widget_set_flags (GtkWidget *widget, int flags)
{
	GTK_OBJECT(widget)->flags = flags;
}

int
gtksharp_gtk_widget_style_get_int (GtkWidget *widget, const char *name)
{
	int value;
	gtk_widget_style_get (widget, name, &value, NULL);
	return value;
}
