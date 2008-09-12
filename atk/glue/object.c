/* object.c : Glue for overriding vms of AtkObject
 *
 * Author: Andres G. Aragoneses  <aaragoneses@novell.com>
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


void atksharp_object_override_get_n_children (GType gtype, gpointer cb);

void atksharp_object_override_ref_child (GType gtype, gpointer cb);

void atksharp_object_override_ref_state_set (GType gtype, gpointer cb);

AtkStateSet* atksharp_object_base_ref_state_set (AtkObject *base);

void atksharp_object_override_get_index_in_parent (GType gtype, gpointer cb);

void
atksharp_object_override_get_n_children (GType gtype, gpointer cb)
{
	AtkObjectClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkObjectClass *) klass)->get_n_children = cb;
}

void
atksharp_object_override_ref_child (GType gtype, gpointer cb)
{
	AtkObjectClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkObjectClass *) klass)->ref_child = cb;
}

void
atksharp_object_override_ref_state_set (GType gtype, gpointer cb)
{
	AtkObjectClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkObjectClass *) klass)->ref_state_set = cb;
}

AtkStateSet*
atksharp_object_base_ref_state_set (AtkObject *atk_obj)
{
	AtkObjectClass *parent = g_type_class_peek (ATK_TYPE_OBJECT);
	
	if (parent->ref_state_set)
		return (*parent->ref_state_set) (atk_obj);
	return NULL;
}

void
atksharp_object_override_get_index_in_parent (GType gtype, gpointer cb)
{
	AtkObjectClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkObjectClass *) klass)->get_index_in_parent = cb;
}

