/* unichar.c : Glue to access unichars as strings.
 *
 * Author: Mike Kestner  <mkestner@ximian.com>
 *
 * Copyright <c> 2004 Novell, Inc.
 */


#include <glib.h>

/* Forward declarations */
gchar *gtksharp_unichar_to_utf8_string (gunichar chr);
/* */

gchar * 
gtksharp_unichar_to_utf8_string (gunichar chr)
{
	gchar *buf = g_new0 (gchar, 7);
	gint cnt = g_unichar_to_utf8 (chr, buf);
	buf [cnt] = 0;
	return buf;
}

