/* style.c : Glue to access fields in GtkStyle.
 *
 * Author: Rachel Hestilow  <hestilow@ximian.com>
 *         Radek Doulik <rodo@matfyz.cz>
 *
 * Copyright (c) 2002, 2003 Rachel Hestilow, Mike Kestner, Radek Doulik
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

#include <gtk/gtk.h>

/* Forward declarations */
GdkColor *gtksharp_gtk_style_get_fg (GtkStyle *style, int i);

GdkColor *gtksharp_gtk_style_get_bg (GtkStyle *style, int i);

GdkColor *gtksharp_gtk_style_get_light (GtkStyle *style, int i);

GdkColor *gtksharp_gtk_style_get_mid (GtkStyle *style, int i);

GdkColor *gtksharp_gtk_style_get_dark (GtkStyle *style, int i);

GdkColor *gtksharp_gtk_style_get_text (GtkStyle *style, int i);

GdkColor *gtksharp_gtk_style_get_base (GtkStyle *style, int i);

PangoFontDescription *gtksharp_gtk_style_get_font_description (GtkStyle *style);

int gtksharp_gtk_style_get_thickness (GtkStyle *style, int x);

void gtksharp_gtk_style_set_thickness (GtkStyle *style, int thickness);

/* */

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

GdkColor*
gtksharp_gtk_style_get_light (GtkStyle *style, int i)
{
	return &style->light[i];
}

GdkColor*
gtksharp_gtk_style_get_mid (GtkStyle *style, int i)
{
	return &style->mid[i];
}

GdkColor*
gtksharp_gtk_style_get_dark (GtkStyle *style, int i)
{
	return &style->dark[i];
}

GdkColor*
gtksharp_gtk_style_get_text (GtkStyle *style, int i)
{
	return &style->text[i];
}

GdkColor*
gtksharp_gtk_style_get_base (GtkStyle *style, int i)
{
	return &style->base[i];
}

PangoFontDescription *
gtksharp_gtk_style_get_font_description (GtkStyle *style)
{
	return style->font_desc;
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


