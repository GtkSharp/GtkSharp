/* layoutline.c : Glue to access fields in PangoLayoutLine struct.
 *
 * Author: Jeroen Zwartepoorte  <jeroen@xs4all.nl
 *
 * <c> 2004 Jeroen Zwartepoorte
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
