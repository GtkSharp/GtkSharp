/* util.c : Glue for overriding vms of AtkUtil
 *
 * Author: Mike Kestner  <mkestner@novell.com>
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

#include <atk/atk.h>

void atksharp_util_override_add_global_event_listener (gpointer cb);
void atksharp_util_override_remove_global_event_listener (gpointer cb);
void atksharp_util_override_remove_key_event_listener (gpointer cb);

void
atksharp_util_override_add_global_event_listener (gpointer cb)
{
	AtkUtilClass *klass = g_type_class_peek (ATK_TYPE_UTIL);
	if (!klass)
		klass = g_type_class_ref (ATK_TYPE_UTIL);
	((AtkUtilClass *) klass)->add_global_event_listener = cb;
}

void
atksharp_util_override_remove_global_event_listener (gpointer cb)
{
	AtkUtilClass *klass = g_type_class_peek (ATK_TYPE_UTIL);
	if (!klass)
		klass = g_type_class_ref (ATK_TYPE_UTIL);
	((AtkUtilClass *) klass)->remove_global_event_listener = cb;
}

void
atksharp_util_override_remove_key_event_listener (gpointer cb)
{
	AtkUtilClass *klass = g_type_class_peek (ATK_TYPE_UTIL);
	if (!klass)
		klass = g_type_class_ref (ATK_TYPE_UTIL);
	((AtkUtilClass *) klass)->remove_key_event_listener = cb;
}


