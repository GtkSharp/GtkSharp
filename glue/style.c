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
	return style->white_gc;
}

GdkGC*
gtksharp_gtk_style_get_black_gc (GtkStyle *style)
{
	return style->black_gc;
}

GdkGC**
gtksharp_gtk_style_get_fg_gc (GtkStyle *style)
{
	return style->fg_gc;
}

GdkGC**
gtksharp_gtk_style_get_bg_gc (GtkStyle *style)
{
	return style->bg_gc;
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
gtksharp_gtk_style_get_fg (GtkStyle *style)
{
	return style->fg;
}

GdkColor**
gtksharp_gtk_style_get_bg (GtkStyle *style)
{
	return style->bg;
}

