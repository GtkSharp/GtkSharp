/* dialog.c : Glue for accessing fields in the GtkDialog widget.
 *
 * Author: Duncan Mak  (duncan@ximian.com)
 *
 * (C) Ximian, INc.
 */

#include <gtk/gtkdialog.h>

GtkWidget*
gtksharp_dialog_get_vbox (GtkDialog *dialog)
{
	return dialog->vbox;
}

GtkWidget*
gtksharp_dialog_get_action_area (GtkDialog *dialog)
{
	return dialog->action_area;
}
