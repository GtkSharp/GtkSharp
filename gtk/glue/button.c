/* 
 * button.c : Glue for utility functions for GtkButton
 */

#include <gtk/gtkbutton.h>

/* Forward declarations */
int gtksharp_button_get_in_button (GtkButton* button);
void gtksharp_button_set_in_button (GtkButton* button, int b);

int
gtksharp_button_get_in_button (GtkButton* button)
{
	return button->in_button;
}

void 
gtksharp_button_set_in_button (GtkButton* button, int b)
{
	button->in_button = b;
}
