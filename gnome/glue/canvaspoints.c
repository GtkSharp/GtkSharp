/* canvaspoints.c : Custom ctor for a CanvasPoints struct.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow
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

