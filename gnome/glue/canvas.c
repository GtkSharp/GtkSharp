/* canvas.c : Glue for accessing fields in a GnomeCanvas
 *
 * Author: Mike Kestner (mkestner@ximian.com)
 *
 * Copyright (C) 2004 Novell, Inc.
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

#include <libgnomecanvas/gnome-canvas.h>

gdouble gnomesharp_canvas_get_pixels_per_unit (GnomeCanvas *canvas);

gdouble 
gnomesharp_canvas_get_pixels_per_unit (GnomeCanvas *canvas)
{
	return canvas->pixels_per_unit;
}
