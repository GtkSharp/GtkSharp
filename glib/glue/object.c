/* object.c : Glue to clean up GtkObject references.
 *
 * Author: Mike Kestner <mkestner@speakeasy.net>
 *
 * Copyright (c) 2002 Mike Kestner
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

#include <glib-object.h>

/* Forward declarations */
int      gtksharp_object_get_ref_count (GObject *obj);
GObject *gtksharp_object_newv (GType type, gint cnt, gchar **names, GValue *vals);
/* */

int
gtksharp_object_get_ref_count (GObject *obj)
{
	return obj->ref_count;
}

GObject *
gtksharp_object_newv (GType type, gint cnt, gchar **names, GValue *vals)
{
	int i;
	GParameter *parms = NULL;
	GObject *result;

	if (cnt > 0)
		parms = g_new0 (GParameter, cnt);

	for (i = 0; i < cnt; i++) {
		parms[i].name = names[i];
		parms[i].value = vals[i];
	}

	result = g_object_newv (type, cnt, parms);

	g_free (parms);
	return result;
}

