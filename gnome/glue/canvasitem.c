/* canvasitem.c : Glue for accessing fields in a GnomeCanvasItem
 *
 * Author: Duncan Mak (duncan@ximian.com)
 *
 * (C) Ximian, INc.
 */

#include <libgnomecanvas/gnome-canvas.h>

GnomeCanvas* gtksharp_gnome_canvas_item_get_canvas (GnomeCanvasItem *item);

GnomeCanvas*
gtksharp_gnome_canvas_item_get_canvas (GnomeCanvasItem *item)
{
	return item->canvas;
}


