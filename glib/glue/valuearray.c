/* valuearray.c : Glue to access GValueArray fields
 *
 * Author: Mike Kestner <mkestner@ximian.com>
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

#include <glib-object.h>

GValue *gtksharp_value_array_get_array (GValueArray *va);
guint gtksharp_value_array_get_count (GValueArray *va);

GValue *
gtksharp_value_array_get_array (GValueArray *va)
{
	return va->values;
}

guint
gtksharp_value_array_get_count (GValueArray *va)
{
	return va->n_values;
}

