/* time_t.c : Glue to allocate time_t.
 *
 * Author: Mike Kestner <mkestner@ximian.com>
 *
 * Copyright <c> 2004 Novell, Inc.
 */

#include <glib.h>
#include <time.h>
#include <stdio.h>

/* Forward declarations */
gint gtksharp_time_t_sizeof (void);
void gtksharp_time_t_print (time_t t);

gint
gtksharp_time_t_sizeof ()
{
	return sizeof (time_t);
}

void
gtksharp_time_t_print (time_t t)
{
	printf ("%s\n", ctime (&t));
}
