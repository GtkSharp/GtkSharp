/* style.c : Glue to access fields in GtkStyle.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *
 * <c> 2002 Rachel Hestilow, Mike Kestner
 */

#include <gtk/gtkstyle.h>

/* FIXME: include all fields */

GdkGC*
gtksharp_gtk_style_get_white_gc (GtkStyle *style)
{
	g_object_ref (G_OBJECT (style->white_gc));
	return style->white_gc;
}

GdkGC*
gtksharp_gtk_style_get_black_gc (GtkStyle *style)
{
	g_object_ref (G_OBJECT (style->black_gc));
	return style->black_gc;
}

GdkGC*
gtksharp_gtk_style_get_fg_gc (GtkStyle *style, int i)
{
	g_object_ref (G_OBJECT (style->fg_gc[i]));
        return style->fg_gc[i];
}

GdkGC*
gtksharp_gtk_style_get_bg_gc (GtkStyle *style, int i)
{
	g_object_ref (G_OBJECT (style->bg_gc[i]));
	return style->bg_gc[i];
}

GdkColor*
gtksharp_gtk_style_get_white (GtkStyle *style)
{
	return &style->white;
}

GdkColor*
gtksharp_gtk_style_get_black (GtkStyle *style)
{
	return &style->black;
}

GdkColor*
gtksharp_gtk_style_get_fg (GtkStyle *style, int i)
{
	return &style->fg[i];
}

GdkColor*
gtksharp_gtk_style_get_bg (GtkStyle *style, int i)
{
	return &style->bg[i];
}

int
gtksharp_gtk_style_get_thickness (GtkStyle *style, int x)
{
	if (x)
		return style->xthickness;
	else
		return style->ythickness;
}

void
gtksharp_gtk_style_set_thickness (GtkStyle *style, int thickness)
{
	if (thickness > 0)
		style->xthickness = thickness;
	else
		style->ythickness = -thickness;
}

