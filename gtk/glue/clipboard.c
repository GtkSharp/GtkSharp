/*
 * clipboard.c
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

#include <gtk/gtk.h>

/* Forward declarations */
GSList         *gtksharp_clipboard_target_list_add (GSList *list,
						    char *target,
						    guint flags,
						    guint info);

GtkTargetEntry *gtksharp_clipboard_target_list_to_array (GSList *list);

void            gtksharp_clipboard_target_array_free (GtkTargetEntry *targets);

void            gtksharp_clipboard_target_list_free (GSList *list);

/* */

GSList *
gtksharp_clipboard_target_list_add (GSList *list, char *target, guint flags, guint info)
{
    GtkTargetEntry *entry = g_new0 (GtkTargetEntry, 1);

    entry->target = g_strdup (target);
    entry->flags = flags;
    entry->info = info;

    return g_slist_prepend (list, entry);
}

GtkTargetEntry *
gtksharp_clipboard_target_list_to_array (GSList *list)
{
    GtkTargetEntry *targets;
    GSList *iter;
    int i;

    targets = g_new0 (GtkTargetEntry, g_slist_length (list));
    for (iter = list, i = 0; iter; iter = iter->next, i++) {
        GtkTargetEntry *t = (GtkTargetEntry *) iter->data;
        targets[i].target = t->target; /* NOT COPIED */
        targets[i].flags = t->flags;
        targets[i].info = t->info;
    }

    return targets;
}

void
gtksharp_clipboard_target_array_free (GtkTargetEntry *targets)
{
    g_free (targets);
}

void
gtksharp_clipboard_target_list_free (GSList *list)
{
    GSList *iter;

    for (iter = list; iter; iter = iter->next) {
        GtkTargetEntry *t = (GtkTargetEntry *) iter->data;
        g_free (t->target);
        g_free (t);
    }

    g_slist_free (list);
}

