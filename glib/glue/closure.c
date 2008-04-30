/* closure.c : Native closure implementation
 *
 * Author: Mike Kestner <mkestner@novell.com>
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

/* Forward declarations */
GClosure* glibsharp_closure_new (GClosureMarshal marshaler, GClosureNotify notify, gpointer data);
/* */

GClosure*
glibsharp_closure_new (GClosureMarshal marshaler, GClosureNotify notify, gpointer data)
{
	GClosure *closure = g_closure_new_simple (sizeof (GClosure), data);
	g_closure_set_marshal (closure, marshaler);
	g_closure_add_finalize_notifier (closure, data, notify);
	return closure;
}
