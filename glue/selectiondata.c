/* selectiondata.c : Glue to access fields of GtkSelectionData
 *
 * Author: Mike Kestner  <mkestner@speakeasy.net>
 *
 * <c> 2003 Novell, Inc.
 */

#include <gtk/gtkwidget.h>
#include <gtk/gtkselection.h>

gint gtksharp_gtk_selection_data_get_length (GtkSelectionData *data);
gint gtksharp_gtk_selection_data_get_format (GtkSelectionData *data);
guchar *gtksharp_gtk_selection_data_get_data_pointer (GtkSelectionData *data);

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

