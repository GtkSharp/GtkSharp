/* colorseldialog.c : Glue for accessing fields in the GtkColorSelectionDialog widget.
 *
 * Author: Duncan Mak  (duncan@ximian.com)
 *
 * (C) Ximian, INc.
 */

#include <gtk/gtkcolorseldialog.h>

GtkWidget*
gtksharp_color_selection_dialog_get_colorsel (GtkColorSelectionDialog *dialog)
{
	return dialog->colorsel;
}

GtkWidget*
gtksharp_color_selection_dialog_get_ok_button (GtkColorSelectionDialog *dialog)
{
	return dialog->ok_button;
}

GtkWidget*
gtksharp_color_selection_dialog_get_cancel_button (GtkColorSelectionDialog *dialog)
{
	return dialog->cancel_button;
}

GtkWidget*
gtksharp_color_selection_dialog_get_help_button (GtkColorSelectionDialog *dialog)
{
	return dialog->help_button;
}
