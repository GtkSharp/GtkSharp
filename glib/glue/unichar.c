/* unichar.c : Glue to access unichars as strings.
 *
 * Author: Mike Kestner  <mkestner@ximian.com>
 *
 * Copyright (c) 2004 Novell, Inc.
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


#include <glib.h>

/* Forward declarations */
gchar *gtksharp_unichar_to_utf8_string (gunichar chr);
gunichar glibsharp_utf16_to_unichar (guint16 chr);
gssize glibsharp_strlen (gchar *s);
/* */

gchar * 
gtksharp_unichar_to_utf8_string (gunichar chr)
{
	gchar *buf = g_new0 (gchar, 7);
	gint cnt = g_unichar_to_utf8 (chr, buf);
	buf [cnt] = 0;
	return buf;
}

gunichar
glibsharp_utf16_to_unichar (guint16 chr)
{
	gunichar *ucs4_str;
	gunichar result;

	ucs4_str = g_utf16_to_ucs4 (&chr, 1, NULL, NULL, NULL);
	result = *ucs4_str;
	g_free (ucs4_str);
	return result;
}

gssize 
glibsharp_strlen (gchar *s)
{
	gssize cnt = 0;
	for (cnt = 0; *s; s++, cnt++);
	return cnt;
}

