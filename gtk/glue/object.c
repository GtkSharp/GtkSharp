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
#include <gtk/gtkobject.h>

/* Forward declarations */
void     gtksharp_object_unref_if_floating (GObject *obj);
gboolean gtksharp_object_is_floating (GObject *obj);
/* */

void
gtksharp_object_unref_if_floating (GObject *obj)
{
	if (GTK_OBJECT_FLOATING (obj))
		g_object_unref (obj);
}

gboolean
gtksharp_object_is_floating (GObject *obj)
{
	return GTK_OBJECT_FLOATING (obj);
}

