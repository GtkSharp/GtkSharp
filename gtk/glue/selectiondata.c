/* selectiondata.c : Glue to access fields of GtkSelectionData
 *
 * Author: Mike Kestner  <mkestner@speakeasy.net>
 *
 * Copyright (c) 2003 Novell, Inc.
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
#include <gtk/gtkselection.h>

gint gtksharp_gtk_selection_data_get_length (GtkSelectionData *data);
gint gtksharp_gtk_selection_data_get_format (GtkSelectionData *data);
guchar *gtksharp_gtk_selection_data_get_data_pointer (GtkSelectionData *data);
GdkAtom gtksharp_gtk_selection_data_get_selection (GtkSelectionData *data);
GdkAtom gtksharp_gtk_selection_data_get_target (GtkSelectionData *data);
GdkAtom gtksharp_gtk_selection_data_get_type (GtkSelectionData *data);

guchar *
gtksharp_gtk_selection_data_get_data_pointer (GtkSelectionData *data)
{
	return data->data;
}

gint
gtksharp_gtk_selection_data_get_length (GtkSelectionData *data)
{
	return data->length;
}

gint
gtksharp_gtk_selection_data_get_format (GtkSelectionData *data)
{
	return data->format;
}

GdkAtom 
gtksharp_gtk_selection_data_get_selection (GtkSelectionData *data)
{
	return data->selection;
}

GdkAtom 
gtksharp_gtk_selection_data_get_target (GtkSelectionData *data)
{
	return data->target;
}

GdkAtom 
gtksharp_gtk_selection_data_get_type (GtkSelectionData *data)
{
	return data->type;
}
