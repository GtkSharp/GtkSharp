/*
 * Utility wrapper for the GtkAdjustment
 *
 * (C) 2002 Miguel de Icaza (miguel@ximian.com)
 */

#include <gtk/gtkadjustment.h>

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

