/* 
 * button.c : Glue for utility functions for GtkButton
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
