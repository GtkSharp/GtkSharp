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

#include <gtk/gtkbindings.h>
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
void gtksharp_widget_connect_set_scroll_adjustments_signal (GType gtype, gpointer callback);
void _gtksharp_marshal_VOID__OBJECT_OBJECT (GClosure *closure, GValue *return_value, guint n_param_values, const GValue *param_values, gpointer invocation_hint, gpointer marshal_data);
int gtksharp_gtk_widget_get_flags (GtkWidget *widget);
void gtksharp_gtk_widget_set_flags (GtkWidget *widget, int flags);
int gtksharp_gtk_widget_style_get_int (GtkWidget *widget, const char *name);
void gtksharp_widget_add_binding_signal (GType gtype, const char *sig_name, GCallback cb);
void gtksharp_widget_register_binding (GType gtype, const char *sig_name, guint key, int mod, gpointer data);
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
	widget->window = g_object_ref (window);
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

#include	<glib-object.h>

#define g_marshal_value_peek_object(v)   (v)->data[0].v_pointer

void
_gtksharp_marshal_VOID__OBJECT_OBJECT (GClosure     *closure,
                                       GValue       *return_value,
                                       guint         n_param_values,
                                       const GValue *param_values,
                                       gpointer      invocation_hint,
                                       gpointer      marshal_data)
{
  typedef void (*GMarshalFunc_VOID__OBJECT_OBJECT) (gpointer     data1,
                                                    gpointer     arg_1,
                                                    gpointer     arg_2,
                                                    gpointer     data2);
  register GMarshalFunc_VOID__OBJECT_OBJECT callback;
  register GCClosure *cc = (GCClosure*) closure;
  register gpointer data1, data2;

  g_return_if_fail (n_param_values == 3);

  if (G_CCLOSURE_SWAP_DATA (closure))
    {
      data1 = closure->data;
      data2 = g_value_peek_pointer (param_values + 0);
    }
  else
    {
      data1 = g_value_peek_pointer (param_values + 0);
      data2 = closure->data;
    }
  callback = (GMarshalFunc_VOID__OBJECT_OBJECT) (marshal_data ? marshal_data : cc->callback);

  callback (data1,
            g_marshal_value_peek_object (param_values + 1),
            g_marshal_value_peek_object (param_values + 2),
            data2);
}

void 
gtksharp_widget_connect_set_scroll_adjustments_signal (GType gtype, gpointer cb)
{
	GType parm_types[] = {GTK_TYPE_ADJUSTMENT, GTK_TYPE_ADJUSTMENT};
	GtkWidgetClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	klass->set_scroll_adjustments_signal = g_signal_newv (
		"set_scroll_adjustments", gtype, G_SIGNAL_RUN_LAST,
		g_cclosure_new (cb, NULL, NULL), NULL, NULL, _gtksharp_marshal_VOID__OBJECT_OBJECT,
		G_TYPE_NONE, 2, parm_types);
}

void
gtksharp_widget_add_binding_signal (GType gtype, const gchar *sig_name, GCallback cb)
{
	GType parm_types[] = {G_TYPE_LONG};
	g_signal_newv (sig_name, gtype, G_SIGNAL_RUN_LAST | G_SIGNAL_ACTION, g_cclosure_new (cb, NULL, NULL), NULL, NULL, g_cclosure_marshal_VOID__LONG, G_TYPE_NONE, 1, parm_types);
}

void
gtksharp_widget_register_binding (GType gtype, const gchar *signame, guint key, int mod, gpointer data)
{
	GObjectClass *klass = g_type_class_peek (gtype);
	if (klass == NULL)
		klass = g_type_class_ref (gtype);
	GtkBindingSet *set = gtk_binding_set_by_class (klass);
	gtk_binding_entry_add_signal (set, key, mod, signame, 1, G_TYPE_LONG, data);
}

