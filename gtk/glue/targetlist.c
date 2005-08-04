/*
 * Utilities for GtkTargetList
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

#include <gtk/gtkselection.h>

/* forward declarations */
int gtksharp_target_list_length (GtkTargetList *list);
void gtksharp_target_list_to_entry_array (GtkTargetList *list, GtkTargetEntry *entries);
/* */

int
gtksharp_target_list_length (GtkTargetList *list)
{
	return g_list_length (list->list);
}

void
gtksharp_target_list_to_entry_array (GtkTargetList *list, GtkTargetEntry *entries)
{
	GList *l;
	int i;

	for (l = list->list, i = 0; l; l = l->next, i++) {
		GtkTargetPair *pair = (GtkTargetPair *)l->data;
		entries[i].target = gdk_atom_name (pair->target);
		entries[i].flags = pair->flags;
		entries[i].info = pair->info;
	}
}
