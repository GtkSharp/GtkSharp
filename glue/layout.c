/* layout.c: Glue to access fields in GtkLayout.
 *
 * Author: Ettore Perazzoli  <ettore@perazzoli.org>
 *
 * Copyright (C) 2003 Ettore Perazzoli
 */

#include <gtk/gtklayout.h>


GdkWindow *gtksharp_gtk_layout_get_bin_window (GtkLayout *layout);

GdkWindow *
gtksharp_gtk_layout_get_bin_window (GtkLayout *layout)
{
	return layout->bin_window;
}


