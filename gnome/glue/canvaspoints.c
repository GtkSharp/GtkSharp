/* canvaspoints.c : Custom ctor for a CanvasPoints struct.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow
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
#include <libgnomecanvas/gnome-canvas-util.h>

/* Forward declarations */
GnomeCanvasPoints *gtksharp_gnome_canvas_points_new_from_array (guint num_points,
								double *coords);
/* */

GnomeCanvasPoints*
gtksharp_gnome_canvas_points_new_from_array (guint num_points, double *coords)
{
	int i, len;
	GnomeCanvasPoints *pts = gnome_canvas_points_new (num_points);

	len = num_points * 2;
	for (i = 0; i < len; i++)
		pts->coords[i] = coords[i];

	return pts;
}

