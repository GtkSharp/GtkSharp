/* colorseldialog.c : Glue for accessing fields in the GtkColorSelectionDialog widget.
 *
 * Author: Duncan Mak  (duncan@ximian.com)
 *
 * Copyright (c) Ximian, INc.
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

#include <gtk/gtkcolorseldialog.h>

/* Forward declarations */
GtkWidget *gtksharp_color_selection_dialog_get_colorsel (GtkColorSelectionDialog *dialog);

GtkWidget *gtksharp_color_selection_dialog_get_ok_button (GtkColorSelectionDialog *dialog);

GtkWidget *gtksharp_color_selection_dialog_get_cancel_button (GtkColorSelectionDialog *dialog);

GtkWidget *gtksharp_color_selection_dialog_get_help_button (GtkColorSelectionDialog *dialog);
/* */

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
