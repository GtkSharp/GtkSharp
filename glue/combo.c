/* combo.c : Glue for accessing fields in the GtkCombo widget.
 *
 * Author: Pablo Baena (pbaena@uol.com.ar)
 *
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
