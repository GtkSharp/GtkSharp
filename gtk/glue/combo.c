/* combo.c : Glue for accessing fields in the GtkCombo widget.
 *
 * Author: Pablo Baena (pbaena@uol.com.ar)
 *
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

#include <gtk/gtkcombo.h>

/* Forward declarations */
GtkWidget *gtksharp_combo_get_entry (GtkCombo* combo);
GtkWidget *gtksharp_combo_get_button (GtkCombo* combo);
/* */

GtkWidget* 
gtksharp_combo_get_entry (GtkCombo* combo)
{
	return combo->entry;
}

GtkWidget* 
gtksharp_combo_get_button (GtkCombo* combo)
{
	return combo->button;
}
