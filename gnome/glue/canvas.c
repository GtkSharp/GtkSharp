/* canvas.c : Glue for accessing fields in a GnomeCanvas
 *
 * Author: Mike Kestner (mkestner@ximian.com)
 *
 * Copyright (C) 2004 Novell, Inc.
 */

#include <libgnomecanvas/gnome-canvas.h>

gdouble gnomesharp_canvas_get_pixels_per_unit (GnomeCanvas *canvas);

gdouble 
gnomesharp_canvas_get_pixels_per_unit (GnomeCanvas *canvas)
{
	return canvas->pixels_per_unit;
}
