/*
 * Utility wrapper for the GtkAdjustment
 *
 * Copyright (c) 2002 Miguel de Icaza (miguel@ximian.com)
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

#include <gtk/gtkadjustment.h>

/* Forward declarations */
void    gtksharp_gtk_adjustment_set_bounds (GtkAdjustment *adj,
					    gdouble lower,
					    gdouble upper,
					    gdouble step_increment,
					    gdouble page_increment,
					    gdouble page_size);

/* */

void
gtksharp_gtk_adjustment_set_bounds (GtkAdjustment *adj,
				    gdouble lower, gdouble upper,
				    gdouble step_increment,
				    gdouble page_increment, gdouble page_size)
{
	adj->lower = lower;
	adj->upper = upper;
	adj->step_increment = step_increment;
	adj->page_increment = page_increment;
	adj->page_size = page_size;
	
	gtk_adjustment_changed (adj);
}

