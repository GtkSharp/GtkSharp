/* layoutline.c : Glue to access fields in PangoLayoutLine struct.
 *
 * Author: Jeroen Zwartepoorte  <jeroen@xs4all.nl
 *
 * Copyright (c) 2004 Jeroen Zwartepoorte
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

#include <pango/pango-layout.h>

/* Forward declarations */
PangoLayout *pangosharp_pango_layout_line_get_layout (PangoLayoutLine *line);
gint pangosharp_pango_layout_line_get_start_index (PangoLayoutLine *line);
gint pangosharp_pango_layout_line_get_length (PangoLayoutLine *line);
/* */

PangoLayout *
pangosharp_pango_layout_line_get_layout (PangoLayoutLine *line)
{
	return line->layout;
}

gint
pangosharp_pango_layout_line_get_start_index (PangoLayoutLine *line)
{
	return line->start_index;
}

gint
pangosharp_pango_layout_line_get_length (PangoLayoutLine *line)
{
	return line->length;
}
