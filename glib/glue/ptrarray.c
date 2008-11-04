/* ptrarray.c : Glue to access GPtrArray fields
 *
 * Author: Mike Gorse <mgorse@novell.com>
 *
 * Copyright (c) 2008 Novell, Inc.
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

void *gtksharp_ptr_array_get_array (GPtrArray *pa);
guint gtksharp_ptr_array_get_count (GPtrArray *pa);
void *gtksharp_ptr_array_get_nth (GPtrArray *pa, int index);

void *
gtksharp_ptr_array_get_array (GPtrArray *pa)
{
	return pa->pdata;
}

guint
gtksharp_ptr_array_get_count (GPtrArray *pa)
{
	return pa->len;
}

void *
gtksharp_ptr_array_get_nth (GPtrArray *pa, int index)
{
	return g_ptr_array_index (pa, index);
}

