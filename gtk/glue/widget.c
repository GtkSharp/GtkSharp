/* widget.c : Glue to access fields in GtkWidget.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * Copyright (c) 2002 Rachel Hestilow, Mike Kestner
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of version 2 of the Lesser GNU General 
 * Public License as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the
 * Free Software Foundation, Inc., 59 Temple Place - Suite 330,
 * Boston, MA 02111-1307, USA.
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
