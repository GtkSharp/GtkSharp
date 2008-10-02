/* hyperlink.c : Glue for overriding vms of AtkHyperlink
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

#include <atk/atk.h>


void atksharp_hyperlink_override_get_uri (GType gtype, gpointer cb);

void atksharp_hyperlink_override_get_object (GType gtype, gpointer cb);

void atksharp_hyperlink_override_get_end_index (GType gtype, gpointer cb);

void atksharp_hyperlink_override_get_start_index (GType gtype, gpointer cb);

void atksharp_hyperlink_override_is_valid (GType gtype, gpointer cb);

void atksharp_hyperlink_override_get_n_anchors (GType gtype, gpointer cb);

void atksharp_hyperlink_override_link_state (GType gtype, gpointer cb);

void atksharp_hyperlink_override_is_selected_link (GType gtype, gpointer cb);


void
atksharp_hyperlink_override_get_uri (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->get_uri = cb;
}

void
atksharp_hyperlink_override_get_object (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->get_object = cb;
}


void
atksharp_hyperlink_override_get_end_index (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->get_end_index = cb;
}

void
atksharp_hyperlink_override_get_start_index (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->get_start_index = cb;
}


void
atksharp_hyperlink_override_is_valid (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->is_valid = cb;
}


void
atksharp_hyperlink_override_get_n_anchors (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->get_n_anchors = cb;
}


void
atksharp_hyperlink_override_link_state (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->link_state = cb;
}


void
atksharp_hyperlink_override_is_selected_link (GType gtype, gpointer cb)
{
	AtkHyperlinkClass *klass = g_type_class_peek (gtype);
	if (!klass)
		klass = g_type_class_ref (gtype);
	((AtkHyperlinkClass *) klass)->is_selected_link = cb;
}
